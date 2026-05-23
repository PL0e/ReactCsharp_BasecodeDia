using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using ASI.Basecode.WebApp.Models.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SemestersController : ControllerBase
    {
        private readonly AsiBasecodeDBContext _context;

        public SemestersController(AsiBasecodeDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SemesterResponse>>> GetAll()
        {
            var semesters = await GetSemesterResponsesAsync();
            return Ok(semesters);
        }

        [HttpGet("current")]
        [ProducesResponseType(typeof(SemesterResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SemesterResponse>> GetCurrent()
        {
            var current = await GetCurrentSemesterAsync();
            if (current == null)
            {
                return NotFound(new { message = "No active semester configured." });
            }

            return Ok(current);
        }

        [HttpGet("active")]
        [ProducesResponseType(typeof(SemesterResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<ActionResult<SemesterResponse>> GetActive()
        {
            return GetCurrent();
        }

        [HttpGet("{semesterId:int}")]
        [ProducesResponseType(typeof(SemesterResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SemesterResponse>> GetById(int semesterId)
        {
            var semester = await _context.Semesters.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == semesterId && !x.IsDeleted);
            if (semester == null)
            {
                return NotFound();
            }

            var currentSemesterId = await GetCurrentSemesterIdAsync();
            return Ok(MapSemesterResponse(semester, semester.Id == currentSemesterId));
        }

        private async Task<List<SemesterResponse>> GetSemesterResponsesAsync()
        {
            var semesters = await _context.Semesters.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.IsCurrent)
                .ThenByDescending(x => x.IsActive)
                .ThenByDescending(x => x.Id)
                .ToListAsync();

            var currentSemesterId = SelectCurrentSemester(semesters)?.Id ?? semesters.FirstOrDefault()?.Id;

            return semesters
                .Select(x => MapSemesterResponse(x, x.Id == currentSemesterId))
                .ToList();
        }

        private async Task<SemesterResponse> GetCurrentSemesterAsync()
        {
            var semesters = await _context.Semesters.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.IsCurrent)
                .ThenByDescending(x => x.IsActive)
                .ThenByDescending(x => x.Id)
                .ToListAsync();

            var selected = SelectCurrentSemester(semesters) ?? semesters.FirstOrDefault();
            return selected == null ? null : MapSemesterResponse(selected, true);
        }

        private async Task<int?> GetCurrentSemesterIdAsync()
        {
            var current = await GetCurrentSemesterAsync();
            return current?.SemesterId;
        }

        private static Semester SelectCurrentSemester(IEnumerable<Semester> semesters)
        {
            return semesters.FirstOrDefault(x => x.IsCurrent)
                ?? semesters.FirstOrDefault(x => x.IsActive)
                ?? semesters.FirstOrDefault(IsSemesterInCurrentDateRange)
                ?? semesters.FirstOrDefault();
        }

        private static bool IsSemesterInCurrentDateRange(Semester semester)
        {
            var today = DateTime.UtcNow.Date;
            return TryParseSemesterDate(semester.StartDate, out var startDate)
                && TryParseSemesterDate(semester.EndDate, out var endDate)
                && startDate.Date <= today
                && endDate.Date >= today;
        }

        private static SemesterResponse MapSemesterResponse(Semester semester, bool isCurrent)
        {
            return new SemesterResponse
            {
                SemesterId = semester.Id,
                Name = BuildSemesterName(semester),
                IsCurrent = isCurrent,
                IsActive = semester.IsActive || isCurrent,
                StartDate = TryParseSemesterDate(semester.StartDate, out var startDate) ? startDate : null,
                EndDate = TryParseSemesterDate(semester.EndDate, out var endDate) ? endDate : null
            };
        }

        private static string BuildSemesterName(Semester semester)
        {
            if (!string.IsNullOrWhiteSpace(semester.SemesterName) && !string.IsNullOrWhiteSpace(semester.SchoolYear))
            {
                return $"{semester.SemesterName} AY {semester.SchoolYear}";
            }

            return semester.SemesterName
                   ?? semester.SchoolYear
                   ?? string.Empty;
        }

        private static bool TryParseSemesterDate(string value, out DateTime dateTime)
        {
            return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.RoundtripKind, out dateTime)
                || DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out dateTime);
        }
    }
}
