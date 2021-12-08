namespace ExternalPortal.ViewModels.Shared.Components
{
    public class MultiSelectCheckboxViewModel
    {
        public string Id { get; set; }
        public bool Selected { get; set; }
        public string SelectionClass { get; set; }

        // This must be the same as the parameter name used in POST requests to select items
        public string Name { get; set; }
    }
}
