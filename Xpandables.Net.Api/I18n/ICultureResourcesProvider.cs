
/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
********************************************************************************/
using System.Globalization;

namespace Xpandables.Net.Api.I18n;
public interface ICultureResourcesProvider
{
    public const string DefaultCulture = "fr-FR";
    public static IEnumerable<CultureInfo> GetCultures()
    {
        System.Resources.ResourceManager resourceManager = new(typeof(DataAnnotations));
        foreach (CultureInfo culture in CultureInfo.GetCultures(CultureTypes.AllCultures))
        {
            if (culture.Equals(CultureInfo.InvariantCulture))
                continue;

            if (resourceManager.GetResourceSet(culture, true, false) is not null)
                yield return culture;
        }
    }

    public IEnumerable<CultureInfo> GetApplicationCultures() => GetCultures();
}
