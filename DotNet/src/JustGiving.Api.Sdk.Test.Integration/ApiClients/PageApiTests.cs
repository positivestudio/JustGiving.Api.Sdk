﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using JustGiving.Api.Sdk.ApiClients;
using JustGiving.Api.Sdk.Http;
using JustGiving.Api.Sdk.Model;
using JustGiving.Api.Sdk.Model.Page;
using JustGiving.Api.Sdk.Model.Remember;
using JustGiving.Api.Sdk.Test.Common.Configuration;
using JustGiving.Api.Sdk.Test.Integration.Configuration;
using NUnit.Framework;

namespace JustGiving.Api.Sdk.Test.Integration.ApiClients
{
    [TestFixture]
    public class PageApiTests
    {
        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void Register_WhenProvidedWithValidAuthenticationAndDetails_CreatesANewPage(WireDataFormat format)
        {
            var client = TestContext.CreateClientValidCredentials(format);
            var pageClient = new PageApi(client.HttpChannel);
            var pageShortName = "api-test-" + Guid.NewGuid();
            var pageCreationRequest = new RegisterPageRequest
            {
                ActivityType = ActivityType.OtherCelebration,
                PageShortName = pageShortName,
                PageTitle = "api test",
                EventName = "The Other Occasion of ApTest and APITest",
                CharityId = 2050,
                TargetAmount = 20M,
                EventDate = DateTime.Now.AddDays(5)
            };

            var registrationResponse = pageClient.Create(pageCreationRequest);

            Assert.That(registrationResponse.Next.Uri, Is.StringContaining(pageShortName));
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void Register_WhenProvidedWithValidAuthenticationAndDetailsAndAnEmptyActivityType_CreatesANewPage(WireDataFormat format)
        {
            var client = TestContext.CreateClientValidCredentials(format);
            var pageClient = new PageApi(client.HttpChannel);
            var pageShortName = "api-test-" + Guid.NewGuid();
            var pageCreationRequest = new RegisterPageRequest
            {
                ActivityType = null,
                PageShortName = pageShortName,
                PageTitle = "When Provided With Valid Authentication Details And An Empty Activity Type - Creates New Page",
                EventName = "The Other Occasion of ApTest and APITest",
                CharityId = 2050,
                EventId = TestConfigurationsHelper.GetProperty<ITestConfigurations, int>(x => x.ValidEventId),
                TargetAmount = 20M,
                EventDate = DateTime.Now.AddDays(5)
            };

            var registrationResponse = pageClient.Create(pageCreationRequest);

            Assert.That(registrationResponse.Next.Uri, Is.StringContaining(pageShortName));
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void RegisterWhenProvidedWithADomainThatDoesNotExist_ThrowsException(WireDataFormat format)
        {
            const string domainThatDoesNotExistOnJustGiving = "Incorrect.com";

            var client = TestContext.CreateClientValidCredentials(format);
            client.SetWhiteLabelDomain(domainThatDoesNotExistOnJustGiving);

            var pageClient = new PageApi(client.HttpChannel);
            var pageShortName = "api-test-" + Guid.NewGuid();
            var pageCreationRequest = new RegisterPageRequest
            {
                ActivityType = null,
                PageShortName = pageShortName,
                PageTitle = "api test",
                EventName = "The Other Occasion of ApTest and APITest",
                CharityId = 2050,
                EventId = 1,
                TargetAmount = 20M,
                EventDate = DateTime.Now.AddDays(5)
            };

            var exception = Assert.Throws<ErrorResponseException>(() => pageClient.Create(pageCreationRequest));

            Assert.That(exception.Response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void Register_WhenProvidedWithValidAuthenticationAndDetailsAndAnEmptyActivityType_TheResponseContainsThePageId(WireDataFormat format)
        {
            var client = TestContext.CreateClientValidCredentials(format);
            var pageClient = new PageApi(client.HttpChannel);
            var pageShortName = "api-test-" + Guid.NewGuid();
            var pageCreationRequest = new RegisterPageRequest
            {
                ActivityType = null,
                PageShortName = pageShortName,
                PageTitle = "api test",
                EventName = "The Other Occasion of ApTest and APITest",
                CharityId = 2050,
                EventId = TestConfigurationsHelper.GetProperty<ITestConfigurations, int>(x => x.ValidEventId),
                TargetAmount = 20M,
                EventDate = DateTime.Now.AddDays(5)
            };

            var registrationResponse = pageClient.Create(pageCreationRequest);

            Assert.That(registrationResponse.PageId != 0);
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void Register_SuppliedValidAuthenticationAndValidRegisterPageRequestWithCompanyAppealId_CanRetrieveCompanyId(WireDataFormat format)
        {
            //arrange
            var client = TestContext.CreateClientValidCredentials(format);
            var pageClient = new PageApi(client.HttpChannel);
            var pageShortName = "api-test-" + Guid.NewGuid();
            const int companyAppealId = 200002;
            var pageCreationRequest = new RegisterPageRequest
            {
                CompanyAppealId = companyAppealId,
                ActivityType = null,
                PageShortName = pageShortName,
                PageTitle = "api test",
                EventName = "The Other Occasion of ApTest and APITest",
                CharityId = 2050,
                EventId = TestConfigurationsHelper.GetProperty<ITestConfigurations, int>(x => x.ValidEventId),
                TargetAmount = 20M,
                EventDate = DateTime.Now.AddDays(5)
            };
            pageClient.Create(pageCreationRequest);

            //act
            var result = pageClient.Retrieve(pageShortName);

            //assert
            Assert.That(result.CompanyAppealId, Is.EqualTo(companyAppealId));
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void Register_SuppliedValidAuthenticationAndValidRegisterPageRequestWithInMemName_CanRetrieveNameFromAttribution(WireDataFormat format)
        {
            var client = TestContext.CreateClientValidCredentials(format);
            var pageClient = new PageApi(client.HttpChannel);
            var pageShortName = "api-test-" + Guid.NewGuid();
            const string inMemName = "Matheu";
            var pageCreationRequest = new RegisterPageRequest
            {
                ActivityType = ActivityType.InMemory,
                Attribution = inMemName,
                PageShortName = pageShortName,
                PageTitle = "api test InMem Name",
                EventName = "The InMem ApiTest",
                CharityId = 2050,
                TargetAmount = 20M,
                EventDate = DateTime.Now.AddDays(5)
            };

            pageClient.Create(pageCreationRequest);
            FundraisingPage page = pageClient.Retrieve(pageShortName);
            Assert.That(page.Attribution, Is.EqualTo(inMemName));
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void SuggestPageShortNames_SuppliedDesiredName_ReturnsSuggestions(WireDataFormat format)
        {
            var client = TestContext.CreateClientValidCredentials(format);
            var pageClient = new PageApi(client.HttpChannel);
            const string desiredName = "david";

            var suggestion = pageClient.SuggestPageShortNames(desiredName);

            Assert.That(suggestion.Names.Length, Is.GreaterThan(0));
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void ListPages_WhenProvidedCredentials_ReturnsPages(WireDataFormat format)
        {
            //arrange
            var client = TestContext.CreateClientValidCredentials(format);
            var pageClient = new PageApi(client.HttpChannel);

            //act
            var result = pageClient.ListAll();

            //assert
            Assert.That(result.Count, Is.GreaterThan(0));
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void RetrivePage_WhenProvidedShortName_ReturnsNotEmptyCollectionOfTeams(WireDataFormat format)
        {
            //arrange
            var client = TestContext.CreateClientNoCredentials(format);
            var pageClient = new PageApi(client.HttpChannel);

            //act
            var result = pageClient.Retrieve("my-test-890");

            //assert
            Assert.IsNotEmpty(result.Teams);
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void RetrievePage_WhenProvidedWithAKnownPage_ReturnsPublicPageView(WireDataFormat format)
        {
            var client = TestContext.CreateClientValidCredentials(format);
            var pageClient = new PageApi(client.HttpChannel);

            // Create Page
            var pageShortName = "api-test-" + Guid.NewGuid();
            var pageCreationRequest = new RegisterPageRequest
                                        {
                                            ActivityType = ActivityType.OtherCelebration,
                                            PageShortName = pageShortName,
                                            PageTitle = "Page Created For Update Story Integration Test",
                                            EventName = "Story Update Testing",
                                            CharityId = 2050,
                                            TargetAmount = 20M,
                                            EventDate = DateTime.Now.AddDays(5),
                                            CustomCodes =
                                                new PageCustomCodes
                                                    {
                                                        CustomCode1 = "code1",
                                                        CustomCode2 = "code2",
                                                        CustomCode3 = "code3",
                                                        CustomCode4 = "code4",
                                                        CustomCode5 = "code5",
                                                        CustomCode6 = "code6"
                                                    }
                                        };
            pageClient.Create(pageCreationRequest);

            // Act
            var pageData = pageClient.Retrieve(pageShortName);

            Assert.NotNull(pageData);
            Assert.That(pageData.PageCreatorName, Is.Not.Empty);
            Assert.AreEqual(pageData.PageShortName, pageCreationRequest.PageShortName);
            Assert.AreEqual(pageData.PageTitle, pageCreationRequest.PageTitle);
            Assert.AreEqual(pageData.EventName, pageCreationRequest.EventName);
            Assert.AreEqual(pageData.TargetAmount, pageCreationRequest.TargetAmount);
            Assert.IsNotNullOrEmpty(pageData.SmsCode);
            Assert.That(pageData.TotalRaisedSms, Is.Not.Empty);
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void RetrievePage_WhenProvidedWithABadPage_ThrowsResourceNotFoundException(WireDataFormat format)
        {
            var client = TestContext.CreateClientNoCredentials(format);
            var pageClient = new PageApi(client.HttpChannel);

            var exception = Assert.Throws<ResourceNotFoundException>(() => pageClient.Retrieve(Guid.NewGuid().ToString()));

            Assert.IsInstanceOf<ResourceNotFoundException>(exception);
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void RetrieveDonationsForPage_WhenProvidedWithAKnownPageAndRequesterIsAnon_ReturnsDonations(WireDataFormat format)
        {
            var client = TestContext.CreateClientNoCredentials(format);
            var pageClient = new PageApi(client.HttpChannel);

            var pageData = pageClient.RetrieveDonationsForPage("rasha25");

            Assert.That(pageData.Donations.Count, Is.GreaterThan(0));
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void RetrieveImagesForPage_WhenProvidedWithAKnownPageAndRequesterIsAnon_ReturnsImages(WireDataFormat format)
        {
            var client = TestContext.CreateClientNoCredentials(format);
            var pageClient = new PageApi(client.HttpChannel);

            var pageData = pageClient.GetImages(new GetFundraisingPageImagesRequest() { PageShortName = "rasha25" });

            Assert.That(pageData.Count, Is.GreaterThan(0));
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void RetrieveVideosForPage_WhenProvidedWithAKnownPageAndRequesterIsAnon_ReturnsVideos(WireDataFormat format)
        {
            var client = TestContext.CreateClientNoCredentials(format);
            var pageClient = new PageApi(client.HttpChannel);

            var pageData = pageClient.GetVideos(new GetFundraisingPageVideosRequest() { PageShortName = "rasha25" });

            Assert.That(pageData.Count, Is.GreaterThan(0));
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void IsPageShortNameRegistered_WhenSuppliedKnownExistingPage_ReturnsTrue(WireDataFormat format)
        {
            var client = TestContext.CreateClientInvalidCredentials(format);
            var pageClient = new PageApi(client.HttpChannel);

            var exists = pageClient.IsPageShortNameRegistered("rasha25");

            Assert.IsTrue(exists);
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void UpdatePageStory_WhenProvidedCredentialsAndValidPage_PostsUpdate(WireDataFormat format)
        {
            var client = TestContext.CreateClientValidCredentials(format);
            var pageClient = new PageApi(client.HttpChannel);

            // Create Page
            var pageShortName = "api-test-" + Guid.NewGuid();
            var pageCreationRequest = new RegisterPageRequest
            {
                ActivityType = ActivityType.OtherCelebration,
                PageShortName = pageShortName,
                PageTitle = "Page Created For Update Story Integration Test",
                EventName = "Story Update Testing",
                CharityId = 2050,
                TargetAmount = 20M,
                EventDate = DateTime.Now.AddDays(5)
            };

            pageClient.Create(pageCreationRequest);

            // Act
            var update = DateTime.Now + ": Unit Test Update";
            pageClient.UpdateStory(pageCreationRequest.PageShortName, update);

            // Assert
            var pageData = pageClient.Retrieve(pageShortName);
            Assert.That(pageData.Story, Is.StringContaining(update));
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void UploadImage_WhenProvidedCredentialsAndValidPageAndImage_UploadsImage(WireDataFormat format)
        {
            var client = TestContext.CreateClientValidCredentials(format);
            var pageClient = new PageApi(client.HttpChannel);

            // Create Page
            var pageCreationRequest = new RegisterPageRequest
            {
                ActivityType = ActivityType.OtherCelebration,
                PageShortName = "api-test-" + Guid.NewGuid(),
                PageTitle = "Page Created For Update Story Integration Test",
                EventName = "Story Update Testing",
                CharityId = 2050,
                TargetAmount = 20M,
                EventDate = DateTime.Now.AddDays(5)
            };
            pageClient.Create(pageCreationRequest);

            var imageName = Guid.NewGuid().ToString();
            pageClient.UploadImage(pageCreationRequest.PageShortName, imageName, File.ReadAllBytes("jpg.jpg"), "image/jpeg");

            // Assert
            var pageData = pageClient.Retrieve(pageCreationRequest.PageShortName);
            Assert.That(pageData.Media.Images[0].Caption, Is.StringContaining(imageName));
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void UploadImage_WhenProvidedInvaildCredentialsAndValidPageAndImage_ThrowsException(WireDataFormat format)
        {
            var client = TestContext.CreateClientInvalidCredentials(format);
            var pageClient = new PageApi(client.HttpChannel);

            var exception = Assert.Throws<ErrorResponseException>(() => pageClient.UploadImage("rasha25", "my image", File.ReadAllBytes("jpg.jpg"), "image/jpeg"));

            Assert.That(exception.Response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void UploadImage_WhenProvidedVaildCredentialsAndInvalidPage_ThrowsException(WireDataFormat format)
        {
            var client = TestContext.CreateClientValidCredentials(format);
            var pageClient = new PageApi(client.HttpChannel);

            var exception = Assert.Throws<ErrorResponseException>(() => pageClient.UploadImage(Guid.NewGuid().ToString(), "my image", File.ReadAllBytes("jpg.jpg"), "image/jpeg"));

            Assert.That(exception.Response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void IsPageShortNameRegistered_WhenSuppliedPageNameUnlikelyToExist_ReturnsFalse(WireDataFormat format)
        {
            var client = TestContext.CreateClientInvalidCredentials(format);
            var pageClient = new PageApi(client.HttpChannel);

            var exists = pageClient.IsPageShortNameRegistered(Guid.NewGuid().ToString());

            Assert.IsFalse(exists);
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void IsPageShortNameRegistered_WhenSuppliedPageNameUnlikelyToExistOnNonDefaultDomain_ReturnsFalse(WireDataFormat format)
        {
            //arrange
            var client = TestContext.CreateClientInvalidCredentials(format);
            var pageClient = new PageApi(client.HttpChannel);

            //act
            var exists = pageClient.IsPageShortNameRegistered("rasha25", TestConfigurationsHelper.GetProperty<ITestConfigurations, string>(x => x.RflDomain));

            //assert
            Assert.IsFalse(exists);
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void IsPageShortNameRegistered_WhenSuppliedUnknownDomain_ThrowsException(WireDataFormat format)
        {
            var client = TestContext.CreateClientInvalidCredentials(format);
            var pageClient = new PageApi(client.HttpChannel);
            const string unknownDomain = "unknownDomain.justgiving.com";

            var exception = Assert.Throws<ErrorResponseException>(() => pageClient.IsPageShortNameRegistered("rasha25", unknownDomain));

            Assert.That(exception.Response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void AddFundraisingPageImage_WhenCredentialsValidAndRequestNotValid_ThrowsException(WireDataFormat format)
        {
            //arrange
            var client = TestContext.CreateClientValidCredentials(format);
            var pageClient = new PageApi(client.HttpChannel);
            var pageCreationRequest = ValidRegisterPageRequest();
            pageClient.Create(pageCreationRequest);
            var addImageRequest = new AddFundraisingPageImageRequest { Url = "", Caption = "", PageShortName = pageCreationRequest.PageShortName };

            //act
            var response = Assert.Throws<ErrorResponseException>(() => pageClient.AddImage(addImageRequest));

            //assert
            Assert.That(response.Response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void AddFundraisingPageVideo_WhenCredentialsValidAndRequestNotValid_ThrowsException(WireDataFormat format)
        {
            //arrange
            var client = TestContext.CreateClientValidCredentials(format);
            var pageClient = new PageApi(client.HttpChannel);
            var pageCreationRequest = ValidRegisterPageRequest();
            pageClient.Create(pageCreationRequest);
            var addVideoRequest = new AddFundraisingPageVideoRequest { Url = "", Caption = "", PageShortName = pageCreationRequest.PageShortName };

            //act
            var response = Assert.Throws<ErrorResponseException>(() => pageClient.AddVideo(addVideoRequest));

            //arrange
            Assert.That(response.Response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void AddFundraisingPageImage_WhenCredentialsValidAndRequestValid_ReturnsSuccessful(WireDataFormat format)
        {
            //arrange
            var client = TestContext.CreateClientValidCredentials(format);
            var pageClient = new PageApi(client.HttpChannel);
            var pageCreationRequest = ValidRegisterPageRequest();
            pageClient.Create(pageCreationRequest);
            var validAddImageRequest = ValidAddFundraisingPageImageRequest(pageCreationRequest.PageShortName);

            //act
            var result = pageClient.AddImage(validAddImageRequest);

            //assert
            Assert.IsNotNullOrEmpty(result.Next.Rel);
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void AddFundraisingPageVideo_WhenCredentialsValidAndRequestValid_ReturnsSuccessful(WireDataFormat format)
        {
            //arrange
            var client = TestContext.CreateClientValidCredentials(format);
            var pageClient = new PageApi(client.HttpChannel);
            var pageCreationRequest = ValidRegisterPageRequest();
            pageClient.Create(pageCreationRequest);
            var addVideoRequest = new AddFundraisingPageVideoRequest { Url = "http://www.youtube.com/watch?v=MSxjbF18BBM", Caption = "neckbrace", PageShortName = pageCreationRequest.PageShortName };

            //act
            var result = pageClient.AddVideo(addVideoRequest);

            //assert
            Assert.IsNotNullOrEmpty(result.Next.Rel);
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void Cannot_Create_Page_For_Event_Using_Event_Reference_And_Id(WireDataFormat format)
        {
            var client = TestContext.CreateClientValidCredentials(format);
            var pageClient = new PageApi(client.HttpChannel);
            var pageShortName = "api-test-" + Guid.NewGuid();
            var pageCreationRequest = new RegisterPageRequest
            {
                ActivityType = null,
                PageShortName = pageShortName,
                PageTitle = "When Provided With Valid Authentication Details And An Empty Activity Type - Creates New Page",
                EventName = "The Other Occasion of ApTest and APITest",
                CharityId = 2050,
                EventId = 1,
                TargetAmount = 20M,
                EventDate = DateTime.Now.AddDays(5)
            };

            var ex = Assert.Throws<ErrorResponseException>(() => pageClient.Create("foo", pageCreationRequest));
            Assert.That(ex.Response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void Can_Create_Page_For_Event_Using_Event_Reference(WireDataFormat format)
        {
            var client = TestContext.CreateClientValidCredentials(format);
            var pageClient = new PageApi(client.HttpChannel);
            var pageShortName = "api-test-" + Guid.NewGuid();
            var pageCreationRequest = new RegisterPageRequest
            {
                ActivityType = null,
                PageShortName = pageShortName,
                PageTitle = "When Provided With Valid Authentication Details And An Empty Activity Type - Creates New Page",
                EventName = "The Other Occasion of ApTest and APITest",
                CharityId = 2050,
                TargetAmount = 20M,
                EventDate = DateTime.Now.AddDays(5)
            };

            pageClient.Create("341_RFL2010", pageCreationRequest);
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void Can_Create_Page_With_Custom_Theme(WireDataFormat format)
        {
            Create_Page_With_Custom_Theme(format);
        }

        public string Create_Page_With_Custom_Theme(WireDataFormat format)
        {
            var client = TestContext.CreateClientValidCredentials(format);
            var pageClient = new PageApi(client.HttpChannel);
            var pageShortName = "api-test-" + Guid.NewGuid();
            var pageCreationRequest = new RegisterPageRequest
            {
                ActivityType = ActivityType.OtherCelebration,
                PageShortName = pageShortName,
                PageTitle = "Page with custom theme",
                EventName = "Test",
                CharityId = 2050,
                TargetAmount = 20M,
                EventDate = DateTime.Now.AddDays(5),
                Theme = new PageTheme { BackgroundColour = "#FF0000", ButtonColour = "#FFFF00", ButtonTextColour = "#00FF00", TitleColour = "#0000FF" }
            };

            pageClient.Create(pageCreationRequest);

            return pageShortName;
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void Register_SuppliedValidAuthenticationAndValidRegisterPageRequestWithRememberedPersonId_CanRetrievePageWithRememberedPersonData(WireDataFormat format)
        {
            var guid = Guid.NewGuid();
            var client = TestContext.CreateClientValidCredentials(format);
            var pageClient = new PageApi(client.HttpChannel);
            var pageShortName = "api-test-" + guid;

            var firstName = "FirstName-api-test";
            var lastName = string.Format("Last-{0}", guid);

            string inMemNameAttribution = String.Format("{0} {1}{2}", firstName, lastName, guid).Trim();

            var rememberedPersonReference = new RememberedPersonReference
                                       {
                                           Relationship = "Other",
                                           RememberedPerson = new RememberedPerson
                                                                  {
                                                                      Id = 80,
                                                                      Gender = Gender.Male.ToString()
                                                                  },
                                       };

            var pageCreationRequest = new RegisterPageRequest
            {
                ActivityType = ActivityType.InMemory,
                Attribution = inMemNameAttribution,
                PageShortName = pageShortName,
                PageTitle = "api test InMem Name",
                EventName = "The InMem ApiTest",
                CharityId = 2050,
                TargetAmount = 20M,
                EventDate = DateTime.Now.AddDays(5),
                RememberedPersonReference = rememberedPersonReference,
            };

            pageClient.Create(pageCreationRequest);
            FundraisingPage page = pageClient.Retrieve(pageShortName);

            Assert.NotNull(page.RememberedPersonSummary.Name);
            Assert.That(page.RememberedPersonSummary.Next.Uri, Is.StringContaining(String.Format("remember/{0}", page.RememberedPersonSummary.Id)));
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void Register_SuppliedValidAuthenticationAndValidRegisterPageRequestWithNewRememberedPersonDetails_CanRetrievePageWithRememberedPersonData(WireDataFormat format)
        {
            var guid = Guid.NewGuid();
            var client = TestContext.CreateClientValidCredentials(format);
            var pageClient = new PageApi(client.HttpChannel);
            var pageShortName = "api-test-" + guid;

            var firstName = "FirstName-api-test";
            var lastName = string.Format("Last-{0}", guid);

            string inMemNameAttribution = String.Format("{0} {1}{2}", firstName, lastName, guid).Trim();

            var rememberedPersonReference = new RememberedPersonReference
            {
                Relationship = "Other",
                RememberedPerson = new RememberedPerson
                                       {
                                           FirstName = firstName,
                                           LastName = lastName,
                                           Gender = Gender.Male.ToString(),
                                           Town = String.Format("town-{0}", guid),
                                           DateOfBirth = DateTime.Now.AddYears(-50),
                                           DateOfDeath = DateTime.Now.AddDays(-1),
                                       }
            };

            var pageCreationRequest = new RegisterPageRequest
            {
                ActivityType = ActivityType.InMemory,
                Attribution = inMemNameAttribution,
                PageShortName = pageShortName,
                PageTitle = "api test InMem Name",
                EventName = "The InMem ApiTest",
                CharityId = 2050,
                TargetAmount = 20M,
                EventDate = DateTime.Now.AddDays(5),
                RememberedPersonReference = rememberedPersonReference,
            };

            pageClient.Create(pageCreationRequest);
            FundraisingPage page = pageClient.Retrieve(pageShortName);

            Assert.NotNull(page.RememberedPersonSummary.Name);
            Assert.That(page.RememberedPersonSummary.Next.Uri, Is.StringContaining(String.Format("remember/{0}", page.RememberedPersonSummary.Id)));
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void Register_WithWhatAndWhy_CreatesCorrectly(WireDataFormat format)
        {
            var guid = Guid.NewGuid();
            var client = TestContext.CreateClientValidCredentials(format);
            var pageClient = new PageApi(client.HttpChannel);
            var pageShortName = "api-test-" + guid;

            var pageCreationRequest = new RegisterPageRequest
            {
                ActivityType = ActivityType.OtherCelebration,
                PageShortName = pageShortName,
                PageTitle = "api test Name",
                EventName = "ApiTest",
                PageSummaryWhat = "saving the universe",
                PageSummaryWhy = "because I'm Batman",
                CharityId = 2050,
                TargetAmount = 20M,
                EventDate = DateTime.Now.AddDays(5),
            };

            pageClient.Create(pageCreationRequest);
            var page = pageClient.Retrieve(pageShortName);

            Assert.That(page.PageSummaryWhat, Is.EqualTo("saving the universe"));
            Assert.That(page.PageSummaryWhy, Is.EqualTo("because I'm Batman"));
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void RetrieveDonationsForPageByReference_WhileSuppliedCorrectReference_ReturnDonations(
            WireDataFormat format)
        {
            //arrange
            var client = TestContext.CreateClientNoCredentials(format);
            var fundraisinResources = new PageApi(client.HttpChannel);
            const string validPageShortName = "";
            const string validReference = "";

            //act
            var result = fundraisinResources.RetrieveDonationsForPageByReference(validPageShortName, validReference);

            //assert
            Assert.IsNotNull(result);
            CollectionAssert.IsNotEmpty(result.Donations);
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void PageUpdates_ReturnPageUpdates(WireDataFormat format)
        {
            //arrange
            var client = TestContext.CreateClientNoCredentials(format);
            var fundraisingResources = new PageApi(client.HttpChannel);
            const string validPageShortName = "my-test-8901";
            //act
            var result = fundraisingResources.PageUpdates(validPageShortName);

            //assert
            Assert.IsNotNull(result);
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void PageUpdate_ReturnPageUpdate(WireDataFormat format)
        {
            //arrange
            var client = TestContext.CreateClientNoCredentials(format);
            var fundraisingResources = new PageApi(client.HttpChannel);
            const string validPageShortName = "my-test-8901";
            const int validUpdateId = 100142;

            //act
            var result = fundraisingResources.PageUpdate(validPageShortName, validUpdateId);

            //assert
            Assert.IsNotNull(result);
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void PageUpdatesAddPost_WhileSupportedValidCredentialsAndValidRequest_ReturnTrue(WireDataFormat format)
        {
            //arrange
            var client = TestContext.CreateClientValidCredentials(format);
            var fundraisingResource = new PageApi(client.HttpChannel);
            var validUpdateRequest = ValidUpdateRequest();
            var validRegisterPageRequest = ValidRegisterPageRequest();
            var resultOfRegisterPage = fundraisingResource.Create(validRegisterPageRequest);

            //act
            var result = fundraisingResource.PageUpdatesAddPost(validRegisterPageRequest.PageShortName,
                                                                validUpdateRequest);

            //assert
            Assert.IsTrue(result);
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void DeletePageUpdate_WhileSupportedValidCredentials_ReturnTrue(WireDataFormat format)
        {
            //arrage
            var client = TestContext.CreateClientValidCredentials(format);
            var fundraisingResource = new PageApi(client.HttpChannel);
            var validUpdateRequest = ValidUpdateRequest();
            var validRegisterPageRequest = ValidRegisterPageRequest();
            fundraisingResource.Create(validRegisterPageRequest);
            fundraisingResource.PageUpdatesAddPost(validRegisterPageRequest.PageShortName,
                                                   validUpdateRequest);
            var pageUpdatesResult = fundraisingResource.PageUpdates(validRegisterPageRequest.PageShortName);
            var update = pageUpdatesResult.First();

            //act
            var result = fundraisingResource.DeletePageUpdate(validRegisterPageRequest.PageShortName, update.Id.Value);

            //assert
            Assert.IsTrue(result);
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void AppendToFundraisingPageAttribution_WhileSupportedValidCredentialsAndValidRequest_ReturnTrue(
            WireDataFormat format)
        {
            //arrange
            var client = TestContext.CreateClientValidCredentials(format);
            var fundraisingResources = new PageApi(client.HttpChannel);
            var validRegisterPageRequest = ValidRegisterPageRequest();
            var createdPage = fundraisingResources.Create(validRegisterPageRequest);
            var validRequest = ValidAppendFundraisingPageAttributionRequest();

            //act
            var result = fundraisingResources.AppendToFundraisingPageAttribution(validRegisterPageRequest.PageShortName,
                                                                                validRequest);
            //assert
            Assert.IsTrue(result);
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void FundraisingPageAttribution_WhileSupportedValidCredentials_ReturnFundraisingPageAttribution(WireDataFormat format)
        {
            //arrange
            var client = TestContext.CreateClientValidCredentials(format);
            var fundraisingResources = new PageApi(client.HttpChannel);
            var validRegisterPageRequest = ValidRegisterPageRequest();
            var createdPage = fundraisingResources.Create(validRegisterPageRequest);
            var validRequest = ValidAppendFundraisingPageAttributionRequest();
            var appendAttributeRequest =
                fundraisingResources.AppendToFundraisingPageAttribution(validRegisterPageRequest.PageShortName,
                                                                        validRequest);
            //act
            var result = fundraisingResources.FundraisingPageAttribution(validRegisterPageRequest.PageShortName);

            //assert
            Assert.IsNotNullOrEmpty(result.Attribution);
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void UpdateFundraisingPageAttribution_WhileSupportedValidCredentialsAndValidRequest_ReturnTrue(WireDataFormat format)
        {
            //arrange
            var client = TestContext.CreateClientValidCredentials(format);
            var fundraisingResources = new PageApi(client.HttpChannel);
            var validRegisterPageRequest = ValidRegisterPageRequest();
            fundraisingResources.Create(validRegisterPageRequest);
            var validAppendRequest = ValidAppendFundraisingPageAttributionRequest();
            fundraisingResources.AppendToFundraisingPageAttribution(validRegisterPageRequest.PageShortName,
                                                                    validAppendRequest);
            var validRequest = ValidUpdateFundraisingPageAttributionRequest();

            //act
            var result = fundraisingResources.UpdateFundraisingPageAttribution(validRegisterPageRequest.PageShortName,
                                                                               validRequest);
            //assert
            Assert.IsTrue(result);
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void DeleteFundraisingPageAttribution_WhileSupportedValidCredentials_ReturnTrue(WireDataFormat format)
        {
            //arrange
            var client = TestContext.CreateClientValidCredentials(format);
            var fundraisingResources = new PageApi(client.HttpChannel);
            var validRegisterPageRequest = ValidRegisterPageRequest();
            fundraisingResources.Create(validRegisterPageRequest);
            var validAppendRequest = ValidAppendFundraisingPageAttributionRequest();
            fundraisingResources.AppendToFundraisingPageAttribution(validRegisterPageRequest.PageShortName,
                                                                    validAppendRequest);
            //act
            var result = fundraisingResources.DeleteFundraisingPageAttribution(validRegisterPageRequest.PageShortName);

            //assert
            Assert.IsTrue(result);
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void DeleteImage_WhileSupportedValidCredentialsAndValidRequest_ReturnTrue(WireDataFormat format)
        {
            //arrange
            var client = TestContext.CreateClientValidCredentials(format);
            var fundraisingResources = new PageApi(client.HttpChannel);
            var validRegisterPageRequest = ValidRegisterPageRequest();
            fundraisingResources.Create(validRegisterPageRequest);
            var validAddImageRequest = ValidAddFundraisingPageImageRequest(validRegisterPageRequest.PageShortName);
            fundraisingResources.AddImage(validAddImageRequest);
            var images =
                fundraisingResources.GetImages(new GetFundraisingPageImagesRequest
                    {
                        PageShortName = validRegisterPageRequest.PageShortName
                    });
            var image = images.First();

            //act
            var result = fundraisingResources.DeleteImage(validRegisterPageRequest.PageShortName, image.Url);

            //assert
            Assert.IsTrue(result);
        }

        [TestCase(WireDataFormat.Json)]
        [TestCase(WireDataFormat.Xml)]
        public void CancelPage_WhileSupportedValidCredentials_ReturnTrue(WireDataFormat format)
        {
            //arrange
            var client = TestContext.CreateClientValidCredentials(format);
            var fundraisingResources = new PageApi(client.HttpChannel);
            var validRegisterPageRequest = ValidRegisterPageRequest();
            fundraisingResources.Create(validRegisterPageRequest);

            //act
            var result = fundraisingResources.CancelPage(validRegisterPageRequest.PageShortName);

            //assert
            Assert.IsTrue(result);
        }

        public static RegisterPageRequest ValidRegisterPageRequest()
        {
            return new RegisterPageRequest
                {
                    ActivityType = null,
                    PageShortName = "test-frp-" + Guid.NewGuid(),
                    PageTitle =
                        "When Provided With Valid Authentication Details And An Empty Activity Type - Creates New Page",
                    EventName = "The Other Occasion of ApTest and APITest",
                    CharityId = 2050,
                    EventId = TestConfigurationsHelper.GetProperty<ITestConfigurations, int>(x => x.ValidEventId),
                    TargetAmount = 20M,
                    EventDate = DateTime.Now.AddDays(5)
                };
        }

        public static PageApi.Update ValidUpdateRequest()
        {
            return new PageApi.Update()
                {
                    Message = "Unit test story"
                };
        }

        public static PageApi.UpdateFundraisingPageAttributionRequest ValidAppendFundraisingPageAttributionRequest()
        {
            return new PageApi.UpdateFundraisingPageAttributionRequest
                {
                    Attribution = "Jon, Jez, Jules"
                };
        }

        public static PageApi.UpdateFundraisingPageAttributionRequest ValidUpdateFundraisingPageAttributionRequest()
        {
            return new PageApi.UpdateFundraisingPageAttributionRequest
                {
                    Attribution = "Jonno, Jezze, Julesse"
                };
        }

        public static AddFundraisingPageImageRequest ValidAddFundraisingPageImageRequest(string pageShortName)
        {
            return new AddFundraisingPageImageRequest
                {
                    Url = "http://placehold.it/350x150",
                    Caption = "test image",
                    PageShortName = pageShortName
                };
        }
    }
}
