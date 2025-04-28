using freelance_marketplace_backend.Models.Dtos;
using freelance_marketplace_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace freelance_marketplace_backend.Data.Repositories
{
    public class UserRepository
    {
        private readonly FreelancingPlatformContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(FreelancingPlatformContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task CreateUserAsync(CreateUserDto user, CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate input
                if (user == null)
                {
                    _logger.LogError("User data is null.");
                    throw new ArgumentNullException(nameof(user), "User data cannot be null.");
                }

                if (string.IsNullOrWhiteSpace(user.UserId))
                {
                    _logger.LogError("UserId is required.");
                    throw new ArgumentException("UserId is required.", nameof(user.UserId));
                }

                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    _logger.LogError("Email is required.");
                    throw new ArgumentException("Email is required.", nameof(user.Email));
                }

                if (string.IsNullOrWhiteSpace(user.Name))
                {
                    _logger.LogError("Name is required.");
                    throw new ArgumentException("Name is required.", nameof(user.Name));
                }

                // Check for duplicate user
                _logger.LogDebug("Checking for duplicate user with ID {UserId}", user.UserId);
                if (await _context.Users.AnyAsync(u => u.Usersid == user.UserId, cancellationToken))
                {
                    _logger.LogWarning("User with ID {UserId} already exists.", user.UserId);
                    throw new InvalidOperationException($"User with ID {user.UserId} already exists.");
                }

                _logger.LogDebug("Checking for duplicate email {Email}", user.Email);
                if (await _context.Users.AnyAsync(u => u.Email == user.Email, cancellationToken))
                {
                    _logger.LogWarning("User with email {Email} already exists.", user.Email);
                    throw new InvalidOperationException($"User with email {user.Email} already exists.");
                }

                // Validate skills
                var userSkills = new List<UsersSkill>();
                if (user.Skills != null && user.Skills.Any())
                {
                    foreach (var skill in user.Skills)
                    {
                        if (skill == null || skill.SkillId <= 0)
                        {
                            _logger.LogError("Invalid skill data provided: {Skill}", skill);
                            throw new ArgumentException("Invalid skill data provided.", nameof(user.Skills));
                        }

                        _logger.LogDebug("Verifying skill with ID {SkillId}", skill.SkillId);
                        var skillExists = await _context.Skills.AnyAsync(s => s.SkillId == skill.SkillId, cancellationToken);
                        if (!skillExists)
                        {
                            _logger.LogWarning("Skill with ID {SkillId} does not exist.", skill.SkillId);
                            throw new InvalidOperationException($"Skill with ID {skill.SkillId} does not exist.");
                        }

                        userSkills.Add(new UsersSkill
                        {
                            SkillId = skill.SkillId,
                            Usersid = user.UserId
                        });
                    }
                }

                // Map DTO to entity
                var newUser = new Models.Entities.User
                {
                    Usersid = user.UserId,
                    Name = user.Name,
                    AboutMe = user.AboutMe,
                    phone = user.Phone,
                    Email = user.Email
                };

                // Use execution strategy for transaction
                var strategy = _context.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                    try
                    {
                        _logger.LogDebug("Adding new user with ID {UserId}", user.UserId);
                        _context.Users.Add(newUser);
                        if (userSkills.Any())
                        {
                            _logger.LogDebug("Adding {SkillCount} skills for user {UserId}", userSkills.Count, user.UserId);
                            _context.UsersSkills.AddRange(userSkills);
                        }

                        await _context.SaveChangesAsync(cancellationToken);
                        await transaction.CommitAsync(cancellationToken);
                        _logger.LogInformation("User {UserId} created successfully.", user.UserId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to save user {UserId}, rolling back transaction.", user.UserId);
                        await transaction.RollbackAsync(cancellationToken);
                        throw;
                    }
                });
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Database operation canceled for user {UserId}: {Message}", user?.UserId ?? "unknown", ex.Message);
                throw new InvalidOperationException("Database operation was canceled due to timeout or cancellation.", ex);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Connection error for user {UserId}: {Message}", user?.UserId ?? "unknown", ex.Message);
                throw new InvalidOperationException($"Failed to create user due to connection issues: {ex.Message}", ex);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to save user {UserId} to database: {Message}", user?.UserId ?? "unknown", ex.InnerException?.Message ?? ex.Message);
                throw new InvalidOperationException($"Failed to save user to database: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {UserId}: {Message}", user?.UserId ?? "unknown", ex.Message);
                throw;
            }
        }
    }
}