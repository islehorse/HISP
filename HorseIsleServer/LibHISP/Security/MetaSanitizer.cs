using HISP.Server;

namespace HISP.Security
{
    public class MetaSanitizer
    {
        // the offical server doesn't do this, but it could be a problem if anyone ever bothered to learn how the game works
        // basically there are a few places where uncontrolled user input, ends up inside the "Meta" window
        // (the one that allows for dynamic codes; prefixed with ^) 
        // 
        // the offical client prevents typing these, but if you send the packet yourself (or use a Nintendo Wii.)
        // the server would accept them, this way we remove any found from the input before processing.
        //
        //
        public static string Sanitize(string s)
        {

            // TODO: maybe a 'fix offical vulnerabilities' ??
            if (!ConfigReader.FixOfficalBugs) return s;

            return s.Replace("^", "");
        }
    }
}
