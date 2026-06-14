using System.ComponentModel.DataAnnotations;

namespace SalasReuniaoApi.Models
{
    public class SalaReuniao
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nome { get; set; } = string.Empty;

        public int Capacidade { get; set; }

        public bool PossuiProjetor { get; set; }
    }
}
