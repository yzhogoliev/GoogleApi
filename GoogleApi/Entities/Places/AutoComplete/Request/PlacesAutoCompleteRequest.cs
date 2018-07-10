﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GoogleApi.Entities.Common;
using GoogleApi.Entities.Common.Enums;
using GoogleApi.Entities.Common.Enums.Extensions;
using GoogleApi.Entities.Interfaces;
using GoogleApi.Entities.Places.AutoComplete.Request.Enums;
using GoogleApi.Entities.Common.Extensions;

namespace GoogleApi.Entities.Places.AutoComplete.Request
{
    /// <summary>
    /// The Google Places API is a service that returns information about a "place" (hereafter referred to as a Place) — defined within this API as an establishment, 
    /// a geographic location, or prominent point of interest — using an HTTP request. Place requests specify locations as latitude/longitude coordinates.
    /// Two basic Place requests are available: a Place Search request and a Place Details request. Generally, a Place Search request is used to return candidate matches, 
    /// while a Place Details request returns more specific information about a Place.
    /// This service is designed for processing place requests generated by a user for placement of application content on a map; 
    /// this service is not designed to respond to batch of offline queries, which are a violation of its terms of use.
    /// </summary>
    public class PlacesAutoCompleteRequest : BasePlacesRequest, IRequestQueryString
    {
        /// <summary>
        /// Base Url.
        /// </summary>
        protected internal override string BaseUrl => base.BaseUrl + "autocomplete/json";

        /// <summary>
        /// The text string on which to search. The Place service will return candidate matches based on this string and order results based on their perceived relevance.
        /// </summary>
        public virtual string Input { get; set; }

        /// <summary>
        /// The character position in the input term at which the service uses text for predictions. 
        /// For example, if the input is 'Googl' and the completion point is 3, the service will match on 'Goo'. 
        /// The offset should generally be set to the position of the text caret. If no offset is supplied, the service will use the entire term.        
        /// </summary>
        public virtual string Offset { get; set; }

        /// <summary>
        /// Place Autocomplete can use session tokens to group together autocomplete requests for billing purposes.
        /// A session consists of the activities required to resolve user input to a place.
        /// When a session token is passed (using the optional sessiontoken parameter), 
        /// autocompleterequests are not billed independently, but are instead billed once after a full 
        /// utocompleteresult is returned. If the sessiontoken parameter is omitted, each request is billed independently.
        /// See the pricing sheet for details.
        /// </summary>
        public virtual string SessionToken { get; set; }

        /// <summary>
        /// The point around which you wish to retrieve Place information.
        /// </summary>
        public virtual Location Location { get; set; }

        /// <summary>
        /// The distance (in meters) within which to return Place results. 
        /// Note that setting a radius biases results to the indicated area, but may not fully restrict results to the specified area. 
        /// See Location Biasing below.
        /// </summary>
        public virtual double? Radius { get; set; }

        /// <summary>
        /// Strictbounds.
        /// Returns only those places that are strictly within the region defined by location and radius. 
        /// This is a restriction, rather than a bias, meaning that results outside this region will not be returned even if they match the user input.
        /// </summary>
        public virtual bool Strictbounds { get; set; }

        /// <summary>
        /// The language in which to return results. See the supported list of domain languages. 
        /// Note that we often update supported languages so this list may not be exhaustive. 
        /// If language is not supplied, the Place service will attempt to use the native language of the domain from which the request is sent.
        /// </summary>
        public virtual Language Language { get; set; } = Language.English;

        /// <summary>
        /// The types of Place results to return. See Place Types below. If no type is specified, all types will be returned.
        /// https://developers.google.com/places/supported_types#table3
        /// </summary>
        public virtual IEnumerable<RestrictPlaceType> Types { get; set; }

        /// <summary>
        /// The component filters, separated by a pipe (|). 
        /// Each component filter consists of a component:value pair and will fully restrict the results from the geocoder. 
        /// For more information see Component Filtering.
        /// </summary>
        public virtual IEnumerable<KeyValuePair<Component, string>> Components { get; set; }

        /// <summary>
        /// See <see cref="BasePlacesRequest.GetQueryStringParameters()"/>.
        /// </summary>
        /// <returns>The <see cref="IList{KeyValuePair}"/>.</returns>
        public override IList<KeyValuePair<string, string>> GetQueryStringParameters()
        {
            var parameters = base.GetQueryStringParameters();

            if (string.IsNullOrEmpty(this.Input))
                throw new ArgumentException("Input is required");

            if (this.Radius.HasValue && (this.Radius > 50000 || this.Radius < 1))
                throw new ArgumentException("Radius must be greater than or equal to 1 and less than or equal to 50.000");

            parameters.Add("input", this.Input);
            parameters.Add("language", this.Language.ToCode());

            if (!string.IsNullOrEmpty(this.Offset))
                parameters.Add("offset", this.Offset);

            if (!string.IsNullOrEmpty(this.SessionToken))
                parameters.Add("sessiontoken", this.SessionToken);
                
            if (this.Location != null)
                parameters.Add("location", this.Location.ToString());

            if (this.Radius.HasValue)
                parameters.Add("radius", this.Radius.Value.ToString(CultureInfo.InvariantCulture));

            if (this.Strictbounds)
                parameters.Add("strictbounds", string.Empty);

            if (this.Types != null && this.Types.Any())
            {
                parameters.Add("types", string.Join("|", this.Types.Select(x =>
                {
                    if (x == RestrictPlaceType.Cities || x == RestrictPlaceType.Regions)
                        return $"({x.ToString().ToLower()})";

                    return $"{x.ToString().ToLower()}";
                })));
            }

            if (this.Components != null && this.Components.Any())
                parameters.Add("components", string.Join("|", this.Components.Select(x => $"{x.Key.ToString().ToLower()}:{x.Value}")));

            return parameters;
        }
    }
}
