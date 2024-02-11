using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Homies.Data.DataConstants;

namespace Homies.Data
{
	public class Event
	{
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(EventMaxName)]
        public string Name { get; set; }

        [Required]
        [MaxLength(EventMaxDescription)]
        public string Description { get; set; }

        [Required]
        public string OrganiserId { get; set; } = string.Empty;

        [Required]
        [ForeignKey(nameof(OrganiserId))]
        public IdentityUser Organiser { get; set; } = null!;

        [Required]
        public DateTime CreatedOn { get; set; }

        [Required]
        public DateTime Start { get; set; }

        [Required]
        public DateTime End { get; set; }

        [Required]
        public int TypeId { get; set; }

        [Required]
        [ForeignKey(nameof(TypeId))]
        public Type Type { get; set; } = null!;


        public IList<EventParticipant> EventsParticipants { get; set; } = new List<EventParticipant>();
    }
}
