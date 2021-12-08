using System.Collections.Generic;

namespace ExternalPortal.ViewModels.Shared.Components
{
    public class FormButtonsViewModel
    {
        public string Message { get; set; }
        public IEnumerable<FormButtonViewModel> Buttons { get; set; } = new List<FormButtonViewModel> { new FormButtonViewModel() };
        public string CancelLinkUrl { get; set; }

        public FormButtonsViewModel()
        {
        }

        public FormButtonsViewModel(string buttonText)
        {
            Buttons = new List<FormButtonViewModel>
            {
                new FormButtonViewModel
                {
                    Text = buttonText
                }
            };
        }

        public FormButtonsViewModel(string buttonText, string style)
        {
            Buttons = new List<FormButtonViewModel>
            {
                new FormButtonViewModel
                {
                    Text = buttonText,
                    CssClasses = style
                }
            };
        }
    }
}
