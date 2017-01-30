using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MS.AspNetCore.Ssl.Mvc {

    /// <summary>
    /// Implements an empty <see cref="IActionResult"/>.
    /// </summary>
    internal class NullResult : IActionResult {

        /// <inheritdoc/>
        public Task ExecuteResultAsync(ActionContext context) => Task.CompletedTask;

    }
}
