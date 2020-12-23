using Mega.Bridge.Models;
using Mega.Bridge.Services;
using System;

namespace Mega.WebService.GraphQL.Utils
{
    public interface IHopexService
    {
        string MwasUrl { get; }
        string HopexSessionToken { get; set; }

        bool TryOpenSession(MwasSettings mwasSettings, MwasSessionConnectionParameters mwasSessionConnectionParameters, int nbRetry = 5, TimeSpan? durationBeforeRetry = null, bool findSession = false, bool useHopexApiMwas = false);
        string CallMacro(string macroId, string data = "", GenerationContext generationContext = null, TimeSpan? timeout = null);
        AsyncMacroResult CallAsyncMacroExecute(string macroId, string data = "", GenerationContext generationContext = null, TimeSpan? timeout = null);
        AsyncMacroResult CallAsyncMacroGetResult(string actionId, GenerationContext generationContext = null, TimeSpan? timeout = null);
        void CloseUpdateSession();
        void CloseSession();
    }

    internal class RealHopexService : IHopexService
    {
        private readonly HopexService _nativeService;

        public string MwasUrl => _nativeService.MwasUrl;

        public string HopexSessionToken { get => _nativeService.HopexSessionToken; set => _nativeService.HopexSessionToken = value; }

        internal RealHopexService(HopexService nativeService)
        {
            _nativeService = nativeService;
        }

        public bool TryOpenSession(MwasSettings mwasSettings, MwasSessionConnectionParameters mwasSessionConnectionParameters, int nbRetry = 5, TimeSpan? durationBeforeRetry = null, bool findSession = false, bool useHopexApiMwas = false)
        {
            return _nativeService.TryOpenSession(mwasSettings, mwasSessionConnectionParameters, nbRetry, durationBeforeRetry, findSession, useHopexApiMwas);
        }

        public string CallMacro(string macroId, string data = "", GenerationContext generationContext = null, TimeSpan? timeout = null)
        {
            return _nativeService.CallMacro(macroId, data, generationContext, timeout);
        }

        public AsyncMacroResult CallAsyncMacroExecute(string macroId, string data = "", GenerationContext generationContext = null, TimeSpan? timeout = null)
        {
            return _nativeService.CallAsyncMacroExecute(macroId, data, generationContext, timeout);
        }

        public AsyncMacroResult CallAsyncMacroGetResult(string actionId, GenerationContext generationContext = null, TimeSpan? timeout = null)
        {
            return _nativeService.CallAsyncMacroGetResult(actionId, generationContext, timeout);
        }

        public void CloseUpdateSession()
        {
            _nativeService.CloseUpdateSession();
        }

        public void CloseSession()
        {
            _nativeService.CloseSession();
        }
    }
}
