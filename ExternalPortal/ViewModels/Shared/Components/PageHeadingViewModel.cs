namespace ExternalPortal.ViewModels.Shared.Components
{
    public class PageHeadingViewModel
    {
        public string Heading { get; set; }
        public string SuperHeading { get; set; }
        public string SubHeading { get; set; }

        public PageHeadingViewModel(string heading)
        {
            Heading = heading;
        }

        public PageHeadingViewModel()
        {
        }
    }
}
