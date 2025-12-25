using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fidelity.Shared.Models
{
    public class CouponAssegnato
    {
        [Key]
        public int Id { get; set; }

        public int CouponId { get; set; }
        [ForeignKey("CouponId")]
        public virtual Coupon Coupon { get; set; } = default!;

        public int ClienteId { get; set; }
        [ForeignKey("ClienteId")]
        public virtual Cliente Cliente { get; set; } = default!;

        public DateTime DataAssegnazione { get; set; }

        public DateTime? DataUtilizzo { get; set; }

        public bool Utilizzato { get; set; } = false;
    }
}
