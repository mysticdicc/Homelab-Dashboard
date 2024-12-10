using System.ComponentModel.DataAnnotations.Schema;

namespace danklibrary
{
    public class DashboardItem
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public required int ID { get; set; }
        public required string DisplayName { get; set; }
        public required string URL { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; } //as bits

        public static bool IsValid(DashboardItem item)
        {
            //validate url
            bool uriValid;

            if (null != item.URL && String.Empty != item.URL)
            {
                uriValid = Uri.TryCreate(item.URL, UriKind.Absolute, out Uri? uri)
                    && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
            }
            else
            {
                uriValid = false;
            }

            //validate name
            bool nameValid;

            if (null != item.DisplayName && String.Empty != item.DisplayName)
            {
                if (item.DisplayName.Length > 50)
                {
                    nameValid = false;
                }
                else
                {
                    nameValid = true;
                }
            }
            else
            {
                nameValid = false;
            }

            //validate description
            bool descriptionValid;

            if (null != item.Description && String.Empty != item.Description)
            {
                if (item.Description.Length > 300)
                {
                    descriptionValid = false;
                }
                else
                {
                    descriptionValid = true;
                }
            }
            else
            {
                descriptionValid = true;
            }

            if (nameValid && descriptionValid && uriValid)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool UrlIsValid(DashboardItem item)
        {
            bool uriValid;

            if (null != item.URL && String.Empty != item.URL)
            {
                uriValid = Uri.TryCreate(item.URL, UriKind.Absolute, out Uri? uri)
                    && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
            }
            else
            {
                uriValid = false;
            }

            if (uriValid)
            {
                return true;
            } 
            else
            {
                return false;
            }
        }
    }
}
