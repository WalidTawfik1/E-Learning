﻿using AutoMapper;
using ElCentre.Core.DTO;
using ElCentre.Core.Entities;
using ElCentre.Core.Interfaces;
using ElCentre.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Infrastructure.Repositories
{
    public class EnrollmentRepository : GenericRepository<Enrollment>, IEnrollmentRepository
    {
        private readonly ElCentreDbContext _context;
        private readonly IMapper _mapper;
        public EnrollmentRepository(ElCentreDbContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<bool> AddEnrollmentAsync(int courseId, string studentId)
        {
            if (courseId <= 0 || string.IsNullOrEmpty(studentId))
                return false;

            var alreadyEnrolled = await IsStudentEnrolledInCourseAsync(studentId, courseId);
            if (alreadyEnrolled)
                return false;

            var enrollment = new Enrollment
            {
                CourseId = courseId,
                StudentId = studentId,
                EnrollmentDate = DateTime.Now,
                Status = "Active",
                Progress = 0
            };
            await base.AddAsync(enrollment);
            return true;
        }

        public async Task<bool> IsStudentEnrolledInCourseAsync(string studentId, int courseId)
        {
            if (string.IsNullOrEmpty(studentId) || courseId <= 0)
                return false;
            var enrollment = await _context.Enrollments
                .Where(x => x.CourseId == courseId && x.StudentId == studentId)
                .FirstOrDefaultAsync();
            return enrollment != null;
        }

        public async Task<bool> MarkLessonAsCompletedAsync(int lessonId, string studentId)
        {
            if (lessonId <= 0 || string.IsNullOrEmpty(studentId))
                return false;

            // Get the lesson to find its course
            var lesson = await _context.Lessons
                .Include(l => l.Module)
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null)
                return false;

            // Get the enrollment
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == lesson.Module.CourseId);

            if (enrollment == null)
                return false;

            // Check if already completed
            var existing = await _context.CompletedLessons
                .FirstOrDefaultAsync(cl => cl.LessonId == lessonId && cl.StudentId == studentId);

            if (existing != null)
                return true; // Already completed

            // Mark as completed
            var completedLesson = new CompletedLesson
            {
                LessonId = lessonId,
                StudentId = studentId,
                EnrollmentId = enrollment.Id,
                CompletedDate = DateTime.Now
            };

            await _context.CompletedLessons.AddAsync(completedLesson);
            await _context.SaveChangesAsync();

            // Update progress for the enrollment
            await CalculateAndUpdateProgressAsync(enrollment.Id);

            return true;
        }

        public async Task<bool> IsLessonCompletedAsync(int lessonId, string studentId)
        {
            if (lessonId <= 0 || string.IsNullOrEmpty(studentId))
                return false;

            return await _context.CompletedLessons
                .AnyAsync(cl => cl.LessonId == lessonId && cl.StudentId == studentId);
        }

        public async Task<List<int>> GetCompletedLessonIdsAsync(string studentId, int courseId)
        {
            // Get all completed lessons for this student in this course
            var completedLessons = await _context.CompletedLessons
                .Include(cl => cl.Lesson)
                .ThenInclude(l => l.Module)
                .Where(cl => cl.StudentId == studentId && cl.Lesson.Module.CourseId == courseId)
                .Select(cl => cl.LessonId)
                .ToListAsync();

            return completedLessons;
        }

        public async Task<float> CalculateAndUpdateProgressAsync(int enrollmentId)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

            if (enrollment == null)
                return 0;

            // Get total lessons in the course
            var totalLessons = await _context.Lessons
                .Include(l => l.Module)
                .Where(l => l.Module.CourseId == enrollment.CourseId)
                .CountAsync();

            if (totalLessons == 0)
                return 0;

            // Get completed lessons count
            var completedLessonsCount = await _context.CompletedLessons
                .Include(cl => cl.Lesson)
                .ThenInclude(l => l.Module)
                .Where(cl => cl.StudentId == enrollment.StudentId &&
                       cl.Lesson.Module.CourseId == enrollment.CourseId)
                .CountAsync();

            // Calculate progress percentage
            float progressPercentage = totalLessons > 0
                ? ((float)completedLessonsCount / totalLessons) * 100
                : 0;

            // Update enrollment
            enrollment.Progress = progressPercentage;

            // Auto-complete if 100% progress
            if (Math.Abs(progressPercentage - 100) < 0.01 && enrollment.Status != "Completed")
            {
                enrollment.Status = "Completed";
                enrollment.CompletionDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return progressPercentage;
        }

        public async Task<List<EnrollmentDTO>> GetStudentEnrollments(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
                return null;

            var enrollments =  await _context.Enrollments
                .Where(e => e.StudentId == studentId)
                .Include(e => e.Course)
                .ToListAsync();
            if (enrollments == null)
                return null;
            var result = _mapper.Map<List<EnrollmentDTO>>(enrollments);
            return result;

        }
    }
}
