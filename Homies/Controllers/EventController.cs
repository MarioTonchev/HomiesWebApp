using Homies.Data;
using Homies.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;
using static Homies.Data.DataConstants;

namespace Homies.Controllers
{
	[Authorize]
	public class EventController : Controller
	{
		private readonly HomiesDbContext context;

        public EventController(HomiesDbContext _context)
        {
            context = _context;
        }

		[HttpGet]
        public async Task<IActionResult> All()
		{
			var events = await context.Events.AsNoTracking()
				.Select(e => new EventInfoViewModel()
			{
				Id = e.Id,
				Name = e.Name,
				Start = e.Start.ToString(DateFormat),
				Type = e.Type.Name,
				Organiser = e.Organiser.UserName
			}).ToListAsync();

			return View(events);
		}

		[HttpGet]
		public async Task<IActionResult> Joined()
		{
			string userId = GetUserId();

			var events = await context.EventsParticipants.Where(ep => ep.HelperId == userId)
				.Select(ep => new EventInfoViewModel(){
					Id = ep.EventId,
					Name = ep.Event.Name,
					Start = ep.Event.Start.ToString(DateFormat),
					Type = ep.Event.Type.Name,
					Organiser = ep.Event.Organiser.UserName
				}).ToListAsync();

			return View(events);
		}

		[HttpPost]
		public async Task<IActionResult> Join(int id)
		{
			var userId = GetUserId();

			var e = await context.Events.Where(e => e.Id == id)
				.Include(e => e.EventsParticipants)
				.FirstOrDefaultAsync();

			if (e == null)
			{
				return BadRequest();
			}

			if (!e.EventsParticipants.Any(p => p.HelperId == userId))
			{
				e.EventsParticipants.Add(new EventParticipant()
				{
					EventId = id,
					HelperId = userId
				});

				await context.SaveChangesAsync();
			}

			return RedirectToAction(nameof(Joined));
		}

		[HttpPost]
		public async Task<IActionResult> Leave(int id)
		{
			var userId = GetUserId();

			var e = await context.Events.Where(e => e.Id == id).Include(e => e.EventsParticipants).FirstOrDefaultAsync();

			if (e == null)
			{
				return BadRequest();
			}

			var ep = e.EventsParticipants.FirstOrDefault(ep => ep.HelperId == userId);

			if (ep == null)
			{
				return BadRequest();
			}

			e.EventsParticipants.Remove(ep);

			await context.SaveChangesAsync();

			return RedirectToAction(nameof(All));
		}

		[HttpGet]
		public async Task<IActionResult> Add()
		{
			var model = new EventFormViewModel();
			model.Types = await GetTypes();

			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> Add(EventFormViewModel model)
		{
			DateTime start = DateTime.Now;
			DateTime end = DateTime.Now;

			if (!DateTime.TryParseExact(model.Start, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out start))
			{
				ModelState.AddModelError(nameof(model.Start), $"Invalid date! Format must be: {DateFormat}");
			}

			if (!DateTime.TryParseExact(model.End, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out end))
			{
				ModelState.AddModelError(nameof(model.End), $"Invalid date! Format must be: {DateFormat}");
			}

			if (!ModelState.IsValid)
			{
				model.Types = await GetTypes();

				return View(model);
			}

			var e = new Event()
			{
				Name = model.Name,
				Description = model.Description,
				CreatedOn = DateTime.Now,
				Start = start,
				End = end,
				OrganiserId = GetUserId(),
				TypeId = model.TypeId,
			};

			await context.Events.AddAsync(e);
			await context.SaveChangesAsync();

			return RedirectToAction(nameof(All));
		}

		[HttpGet]
		public async Task<IActionResult> Edit(int id)
		{
			var e = await context.Events.FindAsync(id);

			if (e == null)
			{
				return BadRequest();
			}

			if (e.OrganiserId != GetUserId())
			{
				return Unauthorized();
			}

			var model = new EventFormViewModel()
			{
				Name = e.Name,
				Description = e.Description,
				Start = e.Start.ToString(DateFormat),
				End = e.End.ToString(DateFormat),
				TypeId = e.TypeId,
				Types = await GetTypes()
			};

			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(EventFormViewModel model, int id)
		{
			var e = await context.Events.FindAsync(id);

			if (e == null)
			{
				return BadRequest();
			}

			if (e.OrganiserId != GetUserId())
			{
				return Unauthorized();
			}

			DateTime start = DateTime.Now;
			DateTime end = DateTime.Now;

			if (!DateTime.TryParseExact(model.Start, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out start))
			{
				ModelState.AddModelError(nameof(model.Start), $"Invalid date! Format must be: {DateFormat}");
			}

			if (!DateTime.TryParseExact(model.End, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out end))
			{
				ModelState.AddModelError(nameof(model.End), $"Invalid date! Format must be: {DateFormat}");
			}

			if (!ModelState.IsValid)
			{
				model.Types = await GetTypes();

				return View(model);
			}

			e.Start = start;
			e.End = end;
			e.Name = model.Name;
			e.Description = model.Description;
			e.TypeId = model.TypeId;

			await context.SaveChangesAsync();

			return RedirectToAction(nameof(All));
		}

		[HttpGet]
		public async Task<IActionResult> Details(int id)
		{
			var model = await context.Events.Where(e => e.Id == id)
			.AsNoTracking()
			.Select(e => new EventDetailsViewModel()
			{
				Id = e.Id,
				Name = e.Name,
				Description = e.Description,
				CreatedOn = e.CreatedOn.ToString(DateFormat),
				Start = e.Start.ToString(DateFormat),
				End = e.End.ToString(DateFormat),
				Organiser = e.Organiser.UserName,
				Type = e.Type.Name
			}).FirstOrDefaultAsync();

			if (model == null)
			{
				return BadRequest();
			}

			return View(model); 
		}

		private string GetUserId()
		{
			return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
		}

		private async Task<IEnumerable<TypeViewModel>> GetTypes()
		{
			return await context.Types.AsNoTracking().Select(t => new TypeViewModel
			{
				Id = t.Id,
				Name = t.Name
			})
			.ToListAsync();
		}
	}
}
