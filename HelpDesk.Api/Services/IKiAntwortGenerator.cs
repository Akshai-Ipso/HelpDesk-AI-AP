namespace HelpDesk.Api.Services
{
    public interface IKiAntwortGenerator
    {
        Task<string> GeneriereVorschlagAsync(
            string titel,
            string beschreibung,
            string kategorie);
    }
}