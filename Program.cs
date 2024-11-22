using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;



// Mushaobs :)


public class Student
{
    public int StudentId { get;  set; }
    public string Name { get; set;}
    public DateTime EnrollmentDate { get; set; }
    public List<Enrollment> Enrollments { get; set; } = new();
}

public class Subject
{
    public int SubjectId { get; set; }
    public string Title { get; set; }
    public int MaximumCapacity { get; set; }
    public List<Enrollment> Enrollments { get; set; } = new();

}

public class Enrollment
{
    public int StudentId { get; set; }
    public Student Student { get; set; }
    public int SubjectId { get; set; }
    public Subject Subject { get; set; }
}


public class AppDbContext : DbContext
{
    public DbSet<Student> Students { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=app.db");
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>()
            .HasKey(e => new { e.StudentId});
        modelBuilder.Entity<Subject>()
            .HasKey(e => new { e.SubjectId});


        modelBuilder.Entity<Enrollment>()
       .HasKey(e => new { e.StudentId, e.SubjectId });


        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId);

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Subject)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.SubjectId);
    }



}

public class Repository
{
    private readonly AppDbContext _context;

    public Repository(AppDbContext context)
    {
        _context = context;
    }

    public void AddSubject(Subject subject)
    {
        _context.Subjects.Add(subject);
        _context.SaveChanges();
    }

    public void AddStudent(Student student)
    {
        _context.Students.Add(student);
        _context.SaveChanges();
    }

    public void EnrollStudentToSubject(int studentId, int subjectId)
    {
        var enrollment = new Enrollment
        {
            StudentId = studentId,
            SubjectId = subjectId
        };
        _context.Enrollments.Add(enrollment);
        _context.SaveChanges();
    }

    public List<Subject> GetAllSubjects()
    {
        return _context.Subjects
            .Include(s => s.Enrollments)
            .ThenInclude(e => e.Student)
            .ToList();
    }

    public List<Student> GetStudentsForSubject(int subjectId)
    {
        return _context.Enrollments
            .Where(e => e.SubjectId == subjectId)
            .Select(e => e.Student)
            .ToList();
    }
}

class Program
{
    static void Main()
    {
        using var context = new AppDbContext();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var repository = new Repository(context);

        var subject = new Subject { Title = "Math", MaximumCapacity = 30 };
        repository.AddSubject(subject);

        var student1 = new Student { Name = "Nika", EnrollmentDate = DateTime.Now };
        var student2 = new Student { Name = "Luka", EnrollmentDate = DateTime.Now };
        repository.AddStudent(student1);
        repository.AddStudent(student2);

        repository.EnrollStudentToSubject(student1.StudentId, subject.SubjectId);
        repository.EnrollStudentToSubject(student2.StudentId, subject.SubjectId);

        var subjects = repository.GetAllSubjects();
        foreach (var subj in subjects)
        {
            Console.WriteLine($"Subject: {subj.Title}");
            foreach (var enrollment in subj.Enrollments)
            {
                Console.WriteLine($" - Student: {enrollment.Student.Name}");
            }
        }
    }
}