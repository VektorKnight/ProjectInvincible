using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace _3rdParty.AsyncAwaitUtil.Source {
    public class WaitForBackgroundThread
    {
        public ConfiguredTaskAwaitable.ConfiguredTaskAwaiter GetAwaiter()
        {
            return Task.Run(() => {}).ConfigureAwait(false).GetAwaiter();
        }
    }
}
