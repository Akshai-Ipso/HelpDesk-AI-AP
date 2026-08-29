namespace HelpDesk.Api.DTOs
{
    public class PagedResultDto<T>
    {
        public IReadOnlyList<T> Elemente { get; set; }
            = Array.Empty<T>();

        public int Seite { get; set; }

        public int Seitengroesse { get; set; }

        public int Gesamtanzahl { get; set; }

        public int Gesamtseiten =>
            Gesamtanzahl == 0
                ? 0
                : (int)Math.Ceiling(
                    Gesamtanzahl / (double)Seitengroesse);
    }
}