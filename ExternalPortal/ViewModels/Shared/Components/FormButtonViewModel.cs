namespace ExternalPortal.ViewModels.Shared.Components
{
    public class FormButtonViewModel
    {
        public string Text { get; set; } = "Submit";
        public string Value { get; set; }
        public string Name { get; set; }
        public string Href { get; set; }
        public bool Disabled { get; set; }
        public string CssClasses { get; set; }
    }
}
