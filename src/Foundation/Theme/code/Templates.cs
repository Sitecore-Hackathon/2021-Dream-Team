using Sitecore.Data;

namespace Foundation.Theme
{
  public struct Templates
  {
        public struct _ThemeSettings
        {
            public static readonly ID TemplateId = new ID("{8E4ACCF3-E2EF-43EC-A34B-B81474676911}");

            public struct Fields
            {
                public static readonly ID LoadSelectedThemesOnContentDelivery = new ID("{E4E1919D-5296-4527-A341-8C87EE6D9453}");
            }
        }

        public struct Styles
        {
            public static readonly ID TemplateId = new ID("{9C0DAC28-08AA-4B44-BC21-53363206F6A5}");
        }

        public struct _ThemeCriticalCss
        {
            public static readonly ID TemplateId = new ID("{9D55EFFB-84BF-46C0-AEA8-C044C1D27289}");

            public struct Fields
            {
                public static readonly ID CriticalCssItems = new ID("{6E72AEF2-ABF0-4885-9B01-1866CE0A61E2}");

                public static readonly ID CriticalCssEnabled = new ID("{038A940B-0A4C-4AB1-94E1-3316D9E6E59B}");
            }
        }

    }
}