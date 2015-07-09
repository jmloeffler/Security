// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Http.Features.Authentication;
using Microsoft.AspNet.Http.Internal;
using Microsoft.Framework.Logging;
using Xunit;

namespace Microsoft.AspNet.Authentication
{
    public class AuthenticationHandlerFacts
    {
        [Fact]
        public void ShouldHandleSchemeAreDeterminedOnlyByMatchingAuthenticationScheme()
        {
            var handler = new TestHandler("Alpha");
            var passiveNoMatch = handler.ShouldHandleScheme("Beta");

            handler = new TestHandler("Alpha");
            var passiveWithMatch = handler.ShouldHandleScheme("Alpha");

            Assert.False(passiveNoMatch);
            Assert.True(passiveWithMatch);
        }

        [Fact]
        public void AutomaticHandlerInAutomaticModeHandlesEmptyChallenges()
        {
            var handler = new TestAutoHandler("ignored", true);
            Assert.True(handler.ShouldHandleScheme(""));
        }

        [Fact]
        public void AutomaticHandlerHandlesNullScheme()
        {
            var handler = new TestAutoHandler("ignored", true);
            Assert.True(handler.ShouldHandleScheme(null));
        }

        [Fact]
        public void AutomaticHandlerIgnoresWhitespaceScheme()
        {
            var handler = new TestAutoHandler("ignored", true);
            Assert.False(handler.ShouldHandleScheme("    "));
        }

        [Fact]
        public void AutomaticHandlerShouldHandleSchemeWhenSchemeMatches()
        {
            var handler = new TestAutoHandler("Alpha", true);
            Assert.True(handler.ShouldHandleScheme("Alpha"));
        }

        [Fact]
        public void AutomaticHandlerShouldNotHandleChallengeWhenSchemeDoesNotMatches()
        {
            var handler = new TestAutoHandler("Dog", true);
            Assert.False(handler.ShouldHandleScheme("Alpha"));
        }

        [Fact]
        public void AutomaticHandlerShouldNotHandleChallengeWhenSchemesNotEmpty()
        {
            var handler = new TestAutoHandler(null, true);
            Assert.False(handler.ShouldHandleScheme("Alpha"));
        }

        [Theory]
        [InlineData("Alpha")]
        [InlineData("")]
        public async Task AuthHandlerAuthenticateCachesTicket(string scheme)
        {
            var handler = new CountHandler(scheme);
            var context = new AuthenticateContext(scheme);
            await handler.AuthenticateAsync(context);
            await handler.AuthenticateAsync(context);
            Assert.Equal(1, handler.AuthCount);
        }

        private class CountHandler : AuthenticationHandler<AuthenticationOptions>
        {
            public int AuthCount = 0;

            public CountHandler(string scheme)
            {
                Initialize(new TestOptions(), new DefaultHttpContext(), new LoggerFactory().CreateLogger("TestHandler"), Framework.WebEncoders.UrlEncoder.Default);
                Options.AuthenticationScheme = scheme;
                Options.AutomaticAuthentication = true;
            }

            protected override Task<AuthenticationTicket> HandleAuthenticateAsync()
            {
                AuthCount++;
                return Task.FromResult(new AuthenticationTicket(null, null));
            }

        }

        private class TestHandler : AuthenticationHandler<AuthenticationOptions>
        {
            public TestHandler(string scheme)
            {
                Initialize(new TestOptions(), new DefaultHttpContext(), new LoggerFactory().CreateLogger("TestHandler"), Framework.WebEncoders.UrlEncoder.Default);
                Options.AuthenticationScheme = scheme;
            }

            protected override Task<AuthenticationTicket> HandleAuthenticateAsync()
            {
                throw new NotImplementedException();
            }
        }

        private class TestOptions : AuthenticationOptions { }

        private class TestAutoOptions : AuthenticationOptions
        {
            public TestAutoOptions()
            {
                AutomaticAuthentication = true;
            }
        }

        private class TestAutoHandler : AuthenticationHandler<TestAutoOptions>
        {
            public TestAutoHandler(string scheme, bool auto)
            {
                Initialize(new TestAutoOptions(), new DefaultHttpContext(), new LoggerFactory().CreateLogger("TestHandler"), Framework.WebEncoders.UrlEncoder.Default);
                Options.AuthenticationScheme = scheme;
                Options.AutomaticAuthentication = auto;
            }

            protected override Task<AuthenticationTicket> HandleAuthenticateAsync()
            {
                throw new NotImplementedException();
            }
        }
    }
}
