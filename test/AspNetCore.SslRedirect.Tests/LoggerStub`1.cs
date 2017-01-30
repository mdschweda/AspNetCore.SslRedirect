using System;
using Microsoft.Extensions.Logging;

namespace MS.AspNetCore.Ssl.Tests {

    public class LoggerStub<T> : ILogger<T> {

        class LoggerStubScope : IDisposable {
            public void Dispose() { }
        }

        public IDisposable BeginScope<TState>(TState state) => new LoggerStubScope();

        public bool IsEnabled(LogLevel logLevel) => false;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }

    }

}
