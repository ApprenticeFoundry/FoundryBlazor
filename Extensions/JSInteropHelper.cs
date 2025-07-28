using Microsoft.JSInterop;
using System.Diagnostics;

namespace eDesignStudio.Components
{
    /// <summary>
    /// Helper class to safely invoke JavaScript interop calls with proper error handling
    /// </summary>
    public static class JSInteropHelper
    {
        /// <summary>
        /// Safely invokes a JavaScript interop method, catching and handling disconnection exceptions
        /// </summary>
        public static async Task<T> SafeInvokeAsync<T>(IJSRuntime jsRuntime, string identifier, params object[] args)
        {
            try
            {
                return await jsRuntime.InvokeAsync<T>(identifier, args);
            }
            catch (JSDisconnectedException ex)
            {
                Debug.WriteLine($"JavaScript disconnection occurred: {ex.Message}");
                // You can log additional details here
                return default!;
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("JavaScript interop call was canceled");
                return default!;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during JavaScript interop: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Safely invokes a JavaScript interop method with void return type, catching and handling disconnection exceptions
        /// </summary>
        public static async Task SafeInvokeVoidAsync(IJSRuntime jsRuntime, string identifier, params object[] args)
        {
            try
            {
                await jsRuntime.InvokeVoidAsync(identifier, args);
            }
            catch (JSDisconnectedException ex)
            {
                Debug.WriteLine($"JavaScript disconnection occurred: {ex.Message}");
                // You can log additional details here
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("JavaScript interop call was canceled");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during JavaScript interop: {ex.Message}");
                throw;
            }
        }
    }
}