﻿using System;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    /// <summary>
    /// Tests the <see cref="AdbCommandLineClient"/> class.
    /// </summary>
    public partial class AdbCommandLineClientTests
    {
        /// <summary>
        /// Tests the <see cref="AdbCommandLineClient.GetVersion"/> method.
        /// </summary>
        [Fact]
        public void GetVersionTest()
        {
            DummyAdbCommandLineClient commandLine = new()
            {
                Version = new Version(1, 0, 32)
            };
            Assert.Equal(new Version(1, 0, 32), commandLine.GetVersion());
        }

        /// <summary>
        /// Tests the <see cref="AdbCommandLineClient.GetVersion"/> method.
        /// </summary>
        [Fact]
        public void GetVersionNullTest()
        {
            DummyAdbCommandLineClient commandLine = new()
            {
                Version = null
            };
            _ = Assert.Throws<AdbException>(commandLine.GetVersion);
        }

        /// <summary>
        /// Tests the <see cref="AdbCommandLineClient.GetVersion"/> method.
        /// </summary>
        [Fact]
        public void GetOutdatedVersionTest()
        {
            DummyAdbCommandLineClient commandLine = new()
            {
                Version = new Version(1, 0, 1)
            };
            _ = Assert.Throws<AdbException>(commandLine.GetVersion);
        }

        /// <summary>
        /// Tests the <see cref="AdbCommandLineClient.StartServer"/> method.
        /// </summary>
        [Fact]
        public void StartServerTest()
        {
            DummyAdbCommandLineClient commandLine = new();
            Assert.False(commandLine.ServerStarted);
            commandLine.StartServer();
            Assert.True(commandLine.ServerStarted);
        }
    }
}
