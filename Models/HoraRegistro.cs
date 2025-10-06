namespace grijalvaApi.Models
{
    public class HoraRegistro
    {
        public int Hora  { get; set; }

        public double NivelAngostura { get; set; }
        public double NivelChicoasen { get; set; }
        public double NivelMalpaso { get; set; }
        public double NivelPenitas { get; set; }
        public double NivelJuanGrijalva { get; set; }

        public Datos refAngostura { get; set; } = new Datos();

        public Datos refChicoasen { get; set; } = new Datos();

        public Datos refMalpaso { get; set; } = new Datos();

        public Datos refPenitas { get; set; } = new Datos();

        public Datos refJuanGrijalva { get; set; } = new Datos();
    }
}
