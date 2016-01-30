﻿using Egnyte.Api.Common;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Egnyte.Api.Links
{
    public class LinksClient
    {
        readonly HttpClient httpClient;

        readonly string domain;

        const string LinkBasePath = "https://{0}.egnyte.com/pubapi/v1/links";

        internal LinksClient(HttpClient httpClient, string domain)
        {
            this.httpClient = httpClient;
            this.domain = domain;
        }

        /// <summary>
        /// Lists all links. Please note, that if the user executing this method is not an admin,
        /// then only links created by the user will be listed.
        /// </summary>
        /// <param name="path">Optional. List links to a file or folder specified by its full path.</param>
        /// <param name="userName">Optional. List links created by this user.</param>
        /// <param name="createdBefore">Optional. List links created before a given date.</param>
        /// <param name="createdAfter">Optional. List links created after a given date.</param>
        /// <param name="linkType">Optional. List links that are "file" or "folder".</param>
        /// <param name="accessibility">Optional. Filter to links whose accessiblity is "anyone,"
        /// "password," "domain," or "recipients."</param>
        /// <param name="offset">Optional. The 0-based index of the initial record being requested.</param>
        /// <param name="count">Optional. Limit number of entries per page. By default,
        /// all entries are returned.</param>
        /// <returns></returns>
        public async Task<LinksList> ListLinks(
            string path = null,
            string userName = null,
            DateTime? createdBefore = null,
            DateTime? createdAfter = null,
            LinkType? linkType = null,
            LinkAccessibility? accessibility = null,
            int? offset = null,
            int? count = null)
        {
            var httpRequest = new HttpRequestMessage(
                HttpMethod.Get,
                ListLinksRequestUri(
                    path,
                    userName,
                    createdBefore,
                    createdAfter,
                    linkType,
                    accessibility,
                    offset,
                    count));

            var serviceHandler = new ServiceHandler<LinksList>(httpClient);
            var response = await serviceHandler.SendRequestAsync(httpRequest).ConfigureAwait(false);

            return response.Data;
        }

        /// <summary>
        /// Gets the details of a link.
        /// </summary>
        /// <param name="linkId">Required. Link id, retrieved earlier from Egnyte.</param>
        /// <returns>Details of the link</returns>
        public async Task<LinkDetails> GetLinkDetails(string linkId)
        {
            if(string.IsNullOrWhiteSpace(linkId))
            {
                throw new ArgumentNullException(nameof(linkId));
            }

            var uriBuilder = new UriBuilder(string.Format(LinkBasePath, domain) + "/" + linkId);
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri);

            var serviceHandler = new ServiceHandler<LinkDetailsResponse>(httpClient);
            var response = await serviceHandler.SendRequestAsync(httpRequest).ConfigureAwait(false);

            return MapGetLinkDetailsResponse(response.Data);
        }

        private LinkDetails MapGetLinkDetailsResponse(LinkDetailsResponse data)
        {
            return new LinkDetails
            {
                Id = data.Id,
                Path = data.Path,
                Url = data.Url,
                Type = ParseLinkType(data.LinkType),
                Accessibility = ParseAccessibility(data.Accessibility),
                Notify = data.Notify,
                Protection = data.Protection,
                LinkToCurrent = data.LinkToCurrent,
                CreationDate = data.CreationDate,
                CreatedBy = data.CreatedBy,
                Recipients = data.Recipients
            };
        }

        LinkAccessibility ParseAccessibility(string accessibility)
        {
            switch (accessibility)
            {
                case "domain":
                    return LinkAccessibility.Domain;
                case "password":
                    return LinkAccessibility.Password;
                case "recipients":
                    return LinkAccessibility.Recipients;
                default:
                    return LinkAccessibility.Anyone;
            }
        }

        LinkType ParseLinkType(string linkType)
        {
            if (linkType == "file")
            {
                return LinkType.File;
            }

            return LinkType.Folder;
        }

        Uri ListLinksRequestUri(
            string path,
            string userName,
            DateTime? createdBefore,
            DateTime? createdAfter,
            LinkType? linkType,
            LinkAccessibility? accessibility,
            int? offset,
            int? count)
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrWhiteSpace(path))
            {
                queryParams.Add("path=" + path);
            }

            if (!string.IsNullOrWhiteSpace(userName))
            {
                queryParams.Add("username=" + userName);
            }

            if (createdBefore.HasValue)
            {
                queryParams.Add("created_before=" + createdBefore.Value.ToString("yyyy-MM-dd"));
            }

            if (createdAfter.HasValue)
            {
                queryParams.Add("created_after=" + createdAfter.Value.ToString("yyyy-MM-dd"));
            }

            if (linkType.HasValue)
            {
                queryParams.Add("type=" + MapLinkType(linkType.Value));
            }

            if (accessibility.HasValue)
            {
                queryParams.Add("accessibility=" + MapAccessibilityType(accessibility.Value));
            }

            if (offset.HasValue)
            {
                queryParams.Add("offset=" + offset.Value);
            }

            if (count.HasValue)
            {
                queryParams.Add("count=" + count);
            }

            var query = string.Join("&", queryParams);

            var uriBuilder = new UriBuilder(string.Format(LinkBasePath, domain))
            {
                Query = query
            };

            return uriBuilder.Uri;
        }

        string MapAccessibilityType(LinkAccessibility value)
        {
            switch (value)
            {
                case LinkAccessibility.Domain:
                    return "domain";
                case LinkAccessibility.Password:
                    return "password";
                case LinkAccessibility.Recipients:
                    return "recipients";
                default:
                    return "anyone";
            }
        }

        string MapLinkType(LinkType linkType)
        {
            if (linkType == LinkType.File)
            {
                return "file";
            }

            return "folder";
        }
    }
}
