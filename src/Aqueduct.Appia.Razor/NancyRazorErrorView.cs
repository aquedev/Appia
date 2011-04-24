using Aqueduct.Appia.Core;

namespace Aqueduct.Appia.Razor
{
    public class NancyRazorErrorView : ViewBase
    {
        public NancyRazorErrorView(string message) {
            this.Message = message;
        }

        public string Message { get; private set; }

        public override void WriteLiteral(object value)
        {
            base.WriteLiteral(Message);
        }

        public override void Execute()
        {
            base.WriteLiteral(this.Message);
        }
    }
}
