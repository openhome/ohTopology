using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using OpenHome.Net.Core;
using OpenHome.Net.ControlPoint;

namespace OpenHome.Net.ControlPoint.Proxies
{
    public interface ICpProxyLinnCoUkSdp1 : ICpProxy, IDisposable
    {
        void SyncOpen();
        void BeginOpen(CpProxy.CallbackAsyncComplete aCallback);
        void EndOpen(IntPtr aAsyncHandle);
        void SyncClose();
        void BeginClose(CpProxy.CallbackAsyncComplete aCallback);
        void EndClose(IntPtr aAsyncHandle);
        void SyncPlay();
        void BeginPlay(CpProxy.CallbackAsyncComplete aCallback);
        void EndPlay(IntPtr aAsyncHandle);
        void SyncStop();
        void BeginStop(CpProxy.CallbackAsyncComplete aCallback);
        void EndStop(IntPtr aAsyncHandle);
        void SyncPause();
        void BeginPause(CpProxy.CallbackAsyncComplete aCallback);
        void EndPause(IntPtr aAsyncHandle);
        void SyncResume();
        void BeginResume(CpProxy.CallbackAsyncComplete aCallback);
        void EndResume(IntPtr aAsyncHandle);
        void SyncSearch(String aSearchType, int aSearchSpeed);
        void BeginSearch(String aSearchType, int aSearchSpeed, CpProxy.CallbackAsyncComplete aCallback);
        void EndSearch(IntPtr aAsyncHandle);
        void SyncSetTrack(int aTrack, int aTitle);
        void BeginSetTrack(int aTrack, int aTitle, CpProxy.CallbackAsyncComplete aCallback);
        void EndSetTrack(IntPtr aAsyncHandle);
        void SyncSetTime(String aTime, int aTitle);
        void BeginSetTime(String aTime, int aTitle, CpProxy.CallbackAsyncComplete aCallback);
        void EndSetTime(IntPtr aAsyncHandle);
        void SyncSetProgramOff();
        void BeginSetProgramOff(CpProxy.CallbackAsyncComplete aCallback);
        void EndSetProgramOff(IntPtr aAsyncHandle);
        void SyncSetProgramInclude(byte[] aIncludeList);
        void BeginSetProgramInclude(byte[] aIncludeList, CpProxy.CallbackAsyncComplete aCallback);
        void EndSetProgramInclude(IntPtr aAsyncHandle);
        void SyncSetProgramExclude(byte[] aExcludeList);
        void BeginSetProgramExclude(byte[] aExcludeList, CpProxy.CallbackAsyncComplete aCallback);
        void EndSetProgramExclude(IntPtr aAsyncHandle);
        void SyncSetProgramRandom();
        void BeginSetProgramRandom(CpProxy.CallbackAsyncComplete aCallback);
        void EndSetProgramRandom(IntPtr aAsyncHandle);
        void SyncSetProgramShuffle();
        void BeginSetProgramShuffle(CpProxy.CallbackAsyncComplete aCallback);
        void EndSetProgramShuffle(IntPtr aAsyncHandle);
        void SyncSetRepeatOff();
        void BeginSetRepeatOff(CpProxy.CallbackAsyncComplete aCallback);
        void EndSetRepeatOff(IntPtr aAsyncHandle);
        void SyncSetRepeatAll();
        void BeginSetRepeatAll(CpProxy.CallbackAsyncComplete aCallback);
        void EndSetRepeatAll(IntPtr aAsyncHandle);
        void SyncSetRepeatTrack();
        void BeginSetRepeatTrack(CpProxy.CallbackAsyncComplete aCallback);
        void EndSetRepeatTrack(IntPtr aAsyncHandle);
        void SyncSetRepeatAb(String aStartTime, String aEndTime);
        void BeginSetRepeatAb(String aStartTime, String aEndTime, CpProxy.CallbackAsyncComplete aCallback);
        void EndSetRepeatAb(IntPtr aAsyncHandle);
        void SyncSetIntroMode(bool aIntroMode, int aOffset, int aSeconds);
        void BeginSetIntroMode(bool aIntroMode, int aOffset, int aSeconds, CpProxy.CallbackAsyncComplete aCallback);
        void EndSetIntroMode(IntPtr aAsyncHandle);
        void SyncSetNext(String aSkip);
        void BeginSetNext(String aSkip, CpProxy.CallbackAsyncComplete aCallback);
        void EndSetNext(IntPtr aAsyncHandle);
        void SyncSetPrev(String aSkip);
        void BeginSetPrev(String aSkip, CpProxy.CallbackAsyncComplete aCallback);
        void EndSetPrev(IntPtr aAsyncHandle);
        void SyncRootMenu();
        void BeginRootMenu(CpProxy.CallbackAsyncComplete aCallback);
        void EndRootMenu(IntPtr aAsyncHandle);
        void SyncTitleMenu();
        void BeginTitleMenu(CpProxy.CallbackAsyncComplete aCallback);
        void EndTitleMenu(IntPtr aAsyncHandle);
        void SyncSetSetupMode(bool aSetupMode);
        void BeginSetSetupMode(bool aSetupMode, CpProxy.CallbackAsyncComplete aCallback);
        void EndSetSetupMode(IntPtr aAsyncHandle);
        void SyncSetAngle(String aSelect, int aIndex);
        void BeginSetAngle(String aSelect, int aIndex, CpProxy.CallbackAsyncComplete aCallback);
        void EndSetAngle(IntPtr aAsyncHandle);
        void SyncSetAudioTrack(String aSelect, int aIndex);
        void BeginSetAudioTrack(String aSelect, int aIndex, CpProxy.CallbackAsyncComplete aCallback);
        void EndSetAudioTrack(IntPtr aAsyncHandle);
        void SyncSetSubtitle(String aSelect, int aIndex);
        void BeginSetSubtitle(String aSelect, int aIndex, CpProxy.CallbackAsyncComplete aCallback);
        void EndSetSubtitle(IntPtr aAsyncHandle);
        void SyncSetZoom(String aSelect, int aIndex);
        void BeginSetZoom(String aSelect, int aIndex, CpProxy.CallbackAsyncComplete aCallback);
        void EndSetZoom(IntPtr aAsyncHandle);
        void SyncSetSacdLayer(String aSacdLayer);
        void BeginSetSacdLayer(String aSacdLayer, CpProxy.CallbackAsyncComplete aCallback);
        void EndSetSacdLayer(IntPtr aAsyncHandle);
        void SyncNavigate(String aNavigation, int aIndex);
        void BeginNavigate(String aNavigation, int aIndex, CpProxy.CallbackAsyncComplete aCallback);
        void EndNavigate(IntPtr aAsyncHandle);
        void SyncSetSlideshow(String aSlideshow);
        void BeginSetSlideshow(String aSlideshow, CpProxy.CallbackAsyncComplete aCallback);
        void EndSetSlideshow(IntPtr aAsyncHandle);
        void SyncSetPassword(String aPassword);
        void BeginSetPassword(String aPassword, CpProxy.CallbackAsyncComplete aCallback);
        void EndSetPassword(IntPtr aAsyncHandle);
        void SyncDiscType(out String aDiscType);
        void BeginDiscType(CpProxy.CallbackAsyncComplete aCallback);
        void EndDiscType(IntPtr aAsyncHandle, out String aDiscType);
        void SyncTitle(out int aTitle);
        void BeginTitle(CpProxy.CallbackAsyncComplete aCallback);
        void EndTitle(IntPtr aAsyncHandle, out int aTitle);
        void SyncTrayState(out String aTrayState);
        void BeginTrayState(CpProxy.CallbackAsyncComplete aCallback);
        void EndTrayState(IntPtr aAsyncHandle, out String aTrayState);
        void SyncDiscState(out String aDiscState);
        void BeginDiscState(CpProxy.CallbackAsyncComplete aCallback);
        void EndDiscState(IntPtr aAsyncHandle, out String aDiscState);
        void SyncPlayState(out String aPlayState);
        void BeginPlayState(CpProxy.CallbackAsyncComplete aCallback);
        void EndPlayState(IntPtr aAsyncHandle, out String aPlayState);
        void SyncSearchType(out String aSearchType);
        void BeginSearchType(CpProxy.CallbackAsyncComplete aCallback);
        void EndSearchType(IntPtr aAsyncHandle, out String aSearchType);
        void SyncSearchSpeed(out int aSearchSpeed);
        void BeginSearchSpeed(CpProxy.CallbackAsyncComplete aCallback);
        void EndSearchSpeed(IntPtr aAsyncHandle, out int aSearchSpeed);
        void SyncTrack(out int aTrack);
        void BeginTrack(CpProxy.CallbackAsyncComplete aCallback);
        void EndTrack(IntPtr aAsyncHandle, out int aTrack);
        void SyncTrackElapsedTime(out String aTrackElapsedTime);
        void BeginTrackElapsedTime(CpProxy.CallbackAsyncComplete aCallback);
        void EndTrackElapsedTime(IntPtr aAsyncHandle, out String aTrackElapsedTime);
        void SyncTrackRemainingTime(out String aTrackRemainingTime);
        void BeginTrackRemainingTime(CpProxy.CallbackAsyncComplete aCallback);
        void EndTrackRemainingTime(IntPtr aAsyncHandle, out String aTrackRemainingTime);
        void SyncDiscElapsedTime(out String aDiscElapsedTime);
        void BeginDiscElapsedTime(CpProxy.CallbackAsyncComplete aCallback);
        void EndDiscElapsedTime(IntPtr aAsyncHandle, out String aDiscElapsedTime);
        void SyncDiscRemainingTime(out String aDiscRemainingTime);
        void BeginDiscRemainingTime(CpProxy.CallbackAsyncComplete aCallback);
        void EndDiscRemainingTime(IntPtr aAsyncHandle, out String aDiscRemainingTime);
        void SyncRepeatMode(out String aRepeatMode);
        void BeginRepeatMode(CpProxy.CallbackAsyncComplete aCallback);
        void EndRepeatMode(IntPtr aAsyncHandle, out String aRepeatMode);
        void SyncIntroMode(out bool aIntroMode);
        void BeginIntroMode(CpProxy.CallbackAsyncComplete aCallback);
        void EndIntroMode(IntPtr aAsyncHandle, out bool aIntroMode);
        void SyncProgramMode(out String aProgramMode);
        void BeginProgramMode(CpProxy.CallbackAsyncComplete aCallback);
        void EndProgramMode(IntPtr aAsyncHandle, out String aProgramMode);
        void SyncDomain(out String aDomain);
        void BeginDomain(CpProxy.CallbackAsyncComplete aCallback);
        void EndDomain(IntPtr aAsyncHandle, out String aDomain);
        void SyncAngle(out int aAngle);
        void BeginAngle(CpProxy.CallbackAsyncComplete aCallback);
        void EndAngle(IntPtr aAsyncHandle, out int aAngle);
        void SyncTotalAngles(out int aTotalAngles);
        void BeginTotalAngles(CpProxy.CallbackAsyncComplete aCallback);
        void EndTotalAngles(IntPtr aAsyncHandle, out int aTotalAngles);
        void SyncSubtitle(out int aSubtitle);
        void BeginSubtitle(CpProxy.CallbackAsyncComplete aCallback);
        void EndSubtitle(IntPtr aAsyncHandle, out int aSubtitle);
        void SyncAudioTrack(out int aAudioTrack);
        void BeginAudioTrack(CpProxy.CallbackAsyncComplete aCallback);
        void EndAudioTrack(IntPtr aAsyncHandle, out int aAudioTrack);
        void SyncZoomLevel(out String aZoomLevel);
        void BeginZoomLevel(CpProxy.CallbackAsyncComplete aCallback);
        void EndZoomLevel(IntPtr aAsyncHandle, out String aZoomLevel);
        void SyncSetupMode(out bool aSetupMode);
        void BeginSetupMode(CpProxy.CallbackAsyncComplete aCallback);
        void EndSetupMode(IntPtr aAsyncHandle, out bool aSetupMode);
        void SyncSacdState(out String aSacdState);
        void BeginSacdState(CpProxy.CallbackAsyncComplete aCallback);
        void EndSacdState(IntPtr aAsyncHandle, out String aSacdState);
        void SyncSlideshow(out String aSlideshow);
        void BeginSlideshow(CpProxy.CallbackAsyncComplete aCallback);
        void EndSlideshow(IntPtr aAsyncHandle, out String aSlideshow);
        void SyncError(out String aError);
        void BeginError(CpProxy.CallbackAsyncComplete aCallback);
        void EndError(IntPtr aAsyncHandle, out String aError);
        void SyncOrientation(out String aOrientation);
        void BeginOrientation(CpProxy.CallbackAsyncComplete aCallback);
        void EndOrientation(IntPtr aAsyncHandle, out String aOrientation);
        void SyncDiscLength(out String aDiscLength);
        void BeginDiscLength(CpProxy.CallbackAsyncComplete aCallback);
        void EndDiscLength(IntPtr aAsyncHandle, out String aDiscLength);
        void SyncTrackLength(out String aTrackLength);
        void BeginTrackLength(CpProxy.CallbackAsyncComplete aCallback);
        void EndTrackLength(IntPtr aAsyncHandle, out String aTrackLength);
        void SyncTotalTracks(out int aTotalTracks);
        void BeginTotalTracks(CpProxy.CallbackAsyncComplete aCallback);
        void EndTotalTracks(IntPtr aAsyncHandle, out int aTotalTracks);
        void SyncTotalTitles(out int aTotalTitles);
        void BeginTotalTitles(CpProxy.CallbackAsyncComplete aCallback);
        void EndTotalTitles(IntPtr aAsyncHandle, out int aTotalTitles);
        void SyncGenre(out String aGenre);
        void BeginGenre(CpProxy.CallbackAsyncComplete aCallback);
        void EndGenre(IntPtr aAsyncHandle, out String aGenre);
        void SyncEncoding(out uint aEncoding);
        void BeginEncoding(CpProxy.CallbackAsyncComplete aCallback);
        void EndEncoding(IntPtr aAsyncHandle, out uint aEncoding);
        void SyncFileSize(out uint aFileSize);
        void BeginFileSize(CpProxy.CallbackAsyncComplete aCallback);
        void EndFileSize(IntPtr aAsyncHandle, out uint aFileSize);
        void SyncDiscId(out uint aDiscId);
        void BeginDiscId(CpProxy.CallbackAsyncComplete aCallback);
        void EndDiscId(IntPtr aAsyncHandle, out uint aDiscId);
        void SyncYear(out String aYear);
        void BeginYear(CpProxy.CallbackAsyncComplete aCallback);
        void EndYear(IntPtr aAsyncHandle, out String aYear);
        void SyncTrackName(out String aTrackName);
        void BeginTrackName(CpProxy.CallbackAsyncComplete aCallback);
        void EndTrackName(IntPtr aAsyncHandle, out String aTrackName);
        void SyncArtistName(out String aArtistName);
        void BeginArtistName(CpProxy.CallbackAsyncComplete aCallback);
        void EndArtistName(IntPtr aAsyncHandle, out String aArtistName);
        void SyncAlbumName(out String aAlbumName);
        void BeginAlbumName(CpProxy.CallbackAsyncComplete aCallback);
        void EndAlbumName(IntPtr aAsyncHandle, out String aAlbumName);
        void SyncComment(out String aComment);
        void BeginComment(CpProxy.CallbackAsyncComplete aCallback);
        void EndComment(IntPtr aAsyncHandle, out String aComment);
        void SyncFileName(int aIndex, out String aFileName);
        void BeginFileName(int aIndex, CpProxy.CallbackAsyncComplete aCallback);
        void EndFileName(IntPtr aAsyncHandle, out String aFileName);
        void SyncSystemCapabilities(out byte[] aSystemCapabilities);
        void BeginSystemCapabilities(CpProxy.CallbackAsyncComplete aCallback);
        void EndSystemCapabilities(IntPtr aAsyncHandle, out byte[] aSystemCapabilities);
        void SyncDiscCapabilities(out byte[] aDiscCapabilities);
        void BeginDiscCapabilities(CpProxy.CallbackAsyncComplete aCallback);
        void EndDiscCapabilities(IntPtr aAsyncHandle, out byte[] aDiscCapabilities);
        void SyncZoomLevelInfo(out byte[] aZoomLevelInfo);
        void BeginZoomLevelInfo(CpProxy.CallbackAsyncComplete aCallback);
        void EndZoomLevelInfo(IntPtr aAsyncHandle, out byte[] aZoomLevelInfo);
        void SyncSubtitleInfo(out byte[] aSubtitleInfo);
        void BeginSubtitleInfo(CpProxy.CallbackAsyncComplete aCallback);
        void EndSubtitleInfo(IntPtr aAsyncHandle, out byte[] aSubtitleInfo);
        void SyncAudioTrackInfo(out byte[] aAudioTrackInfo);
        void BeginAudioTrackInfo(CpProxy.CallbackAsyncComplete aCallback);
        void EndAudioTrackInfo(IntPtr aAsyncHandle, out byte[] aAudioTrackInfo);
        void SyncTableOfContents(out byte[] aTableOfContents);
        void BeginTableOfContents(CpProxy.CallbackAsyncComplete aCallback);
        void EndTableOfContents(IntPtr aAsyncHandle, out byte[] aTableOfContents);
        void SyncDirectoryStructure(out byte[] aDirectoryStructure);
        void BeginDirectoryStructure(CpProxy.CallbackAsyncComplete aCallback);
        void EndDirectoryStructure(IntPtr aAsyncHandle, out byte[] aDirectoryStructure);
        void SetPropertyDiscTypeChanged(System.Action aDiscTypeChanged);
        String PropertyDiscType();
        void SetPropertyTitleChanged(System.Action aTitleChanged);
        int PropertyTitle();
        void SetPropertyTrayStateChanged(System.Action aTrayStateChanged);
        String PropertyTrayState();
        void SetPropertyDiscStateChanged(System.Action aDiscStateChanged);
        String PropertyDiscState();
        void SetPropertyPlayStateChanged(System.Action aPlayStateChanged);
        String PropertyPlayState();
        void SetPropertySearchTypeChanged(System.Action aSearchTypeChanged);
        String PropertySearchType();
        void SetPropertySearchSpeedChanged(System.Action aSearchSpeedChanged);
        int PropertySearchSpeed();
        void SetPropertyTrackChanged(System.Action aTrackChanged);
        int PropertyTrack();
        void SetPropertyRepeatModeChanged(System.Action aRepeatModeChanged);
        String PropertyRepeatMode();
        void SetPropertyIntroModeChanged(System.Action aIntroModeChanged);
        bool PropertyIntroMode();
        void SetPropertyProgramModeChanged(System.Action aProgramModeChanged);
        String PropertyProgramMode();
        void SetPropertyDomainChanged(System.Action aDomainChanged);
        String PropertyDomain();
        void SetPropertyAngleChanged(System.Action aAngleChanged);
        int PropertyAngle();
        void SetPropertyTotalAnglesChanged(System.Action aTotalAnglesChanged);
        int PropertyTotalAngles();
        void SetPropertySubtitleChanged(System.Action aSubtitleChanged);
        int PropertySubtitle();
        void SetPropertyAudioTrackChanged(System.Action aAudioTrackChanged);
        int PropertyAudioTrack();
        void SetPropertyZoomLevelChanged(System.Action aZoomLevelChanged);
        String PropertyZoomLevel();
        void SetPropertySetupModeChanged(System.Action aSetupModeChanged);
        bool PropertySetupMode();
        void SetPropertySacdStateChanged(System.Action aSacdStateChanged);
        String PropertySacdState();
        void SetPropertySlideshowChanged(System.Action aSlideshowChanged);
        String PropertySlideshow();
        void SetPropertyErrorChanged(System.Action aErrorChanged);
        String PropertyError();
        void SetPropertyOrientationChanged(System.Action aOrientationChanged);
        String PropertyOrientation();
        void SetPropertyTotalTracksChanged(System.Action aTotalTracksChanged);
        int PropertyTotalTracks();
        void SetPropertyTotalTitlesChanged(System.Action aTotalTitlesChanged);
        int PropertyTotalTitles();
        void SetPropertyEncodingChanged(System.Action aEncodingChanged);
        uint PropertyEncoding();
        void SetPropertyFileSizeChanged(System.Action aFileSizeChanged);
        uint PropertyFileSize();
        void SetPropertyDiscIdChanged(System.Action aDiscIdChanged);
        uint PropertyDiscId();
        void SetPropertyDiscLengthChanged(System.Action aDiscLengthChanged);
        String PropertyDiscLength();
        void SetPropertyTrackLengthChanged(System.Action aTrackLengthChanged);
        String PropertyTrackLength();
        void SetPropertyGenreChanged(System.Action aGenreChanged);
        String PropertyGenre();
        void SetPropertyYearChanged(System.Action aYearChanged);
        String PropertyYear();
        void SetPropertyTrackNameChanged(System.Action aTrackNameChanged);
        String PropertyTrackName();
        void SetPropertyArtistNameChanged(System.Action aArtistNameChanged);
        String PropertyArtistName();
        void SetPropertyAlbumNameChanged(System.Action aAlbumNameChanged);
        String PropertyAlbumName();
        void SetPropertyCommentChanged(System.Action aCommentChanged);
        String PropertyComment();
        void SetPropertyFileNameChanged(System.Action aFileNameChanged);
        byte[] PropertyFileName();
        void SetPropertySystemCapabilitiesChanged(System.Action aSystemCapabilitiesChanged);
        byte[] PropertySystemCapabilities();
        void SetPropertyDiscCapabilitiesChanged(System.Action aDiscCapabilitiesChanged);
        byte[] PropertyDiscCapabilities();
        void SetPropertyZoomLevelInfoChanged(System.Action aZoomLevelInfoChanged);
        byte[] PropertyZoomLevelInfo();
        void SetPropertySubtitleInfoChanged(System.Action aSubtitleInfoChanged);
        byte[] PropertySubtitleInfo();
        void SetPropertyAudioTrackInfoChanged(System.Action aAudioTrackInfoChanged);
        byte[] PropertyAudioTrackInfo();
        void SetPropertyTableOfContentsChanged(System.Action aTableOfContentsChanged);
        byte[] PropertyTableOfContents();
        void SetPropertyDirectoryStructureChanged(System.Action aDirectoryStructureChanged);
        byte[] PropertyDirectoryStructure();
    }

    internal class SyncOpenLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;

        public SyncOpenLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndOpen(aAsyncHandle);
        }
    };

    internal class SyncCloseLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;

        public SyncCloseLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndClose(aAsyncHandle);
        }
    };

    internal class SyncPlayLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;

        public SyncPlayLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndPlay(aAsyncHandle);
        }
    };

    internal class SyncStopLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;

        public SyncStopLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndStop(aAsyncHandle);
        }
    };

    internal class SyncPauseLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;

        public SyncPauseLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndPause(aAsyncHandle);
        }
    };

    internal class SyncResumeLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;

        public SyncResumeLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndResume(aAsyncHandle);
        }
    };

    internal class SyncSearchLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;

        public SyncSearchLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndSearch(aAsyncHandle);
        }
    };

    internal class SyncSetTrackLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;

        public SyncSetTrackLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndSetTrack(aAsyncHandle);
        }
    };

    internal class SyncSetTimeLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;

        public SyncSetTimeLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndSetTime(aAsyncHandle);
        }
    };

    internal class SyncSetProgramOffLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;

        public SyncSetProgramOffLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndSetProgramOff(aAsyncHandle);
        }
    };

    internal class SyncSetProgramIncludeLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;

        public SyncSetProgramIncludeLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndSetProgramInclude(aAsyncHandle);
        }
    };

    internal class SyncSetProgramExcludeLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;

        public SyncSetProgramExcludeLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndSetProgramExclude(aAsyncHandle);
        }
    };

    internal class SyncSetProgramRandomLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;

        public SyncSetProgramRandomLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndSetProgramRandom(aAsyncHandle);
        }
    };

    internal class SyncSetProgramShuffleLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;

        public SyncSetProgramShuffleLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndSetProgramShuffle(aAsyncHandle);
        }
    };

    internal class SyncSetRepeatOffLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;

        public SyncSetRepeatOffLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndSetRepeatOff(aAsyncHandle);
        }
    };

    internal class SyncSetRepeatAllLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;

        public SyncSetRepeatAllLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndSetRepeatAll(aAsyncHandle);
        }
    };

    internal class SyncSetRepeatTrackLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;

        public SyncSetRepeatTrackLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndSetRepeatTrack(aAsyncHandle);
        }
    };

    internal class SyncSetRepeatAbLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;

        public SyncSetRepeatAbLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndSetRepeatAb(aAsyncHandle);
        }
    };

    internal class SyncSetIntroModeLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;

        public SyncSetIntroModeLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndSetIntroMode(aAsyncHandle);
        }
    };

    internal class SyncSetNextLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;

        public SyncSetNextLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndSetNext(aAsyncHandle);
        }
    };

    internal class SyncSetPrevLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;

        public SyncSetPrevLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndSetPrev(aAsyncHandle);
        }
    };

    internal class SyncRootMenuLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;

        public SyncRootMenuLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndRootMenu(aAsyncHandle);
        }
    };

    internal class SyncTitleMenuLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;

        public SyncTitleMenuLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndTitleMenu(aAsyncHandle);
        }
    };

    internal class SyncSetSetupModeLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;

        public SyncSetSetupModeLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndSetSetupMode(aAsyncHandle);
        }
    };

    internal class SyncSetAngleLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;

        public SyncSetAngleLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndSetAngle(aAsyncHandle);
        }
    };

    internal class SyncSetAudioTrackLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;

        public SyncSetAudioTrackLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndSetAudioTrack(aAsyncHandle);
        }
    };

    internal class SyncSetSubtitleLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;

        public SyncSetSubtitleLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndSetSubtitle(aAsyncHandle);
        }
    };

    internal class SyncSetZoomLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;

        public SyncSetZoomLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndSetZoom(aAsyncHandle);
        }
    };

    internal class SyncSetSacdLayerLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;

        public SyncSetSacdLayerLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndSetSacdLayer(aAsyncHandle);
        }
    };

    internal class SyncNavigateLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;

        public SyncNavigateLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndNavigate(aAsyncHandle);
        }
    };

    internal class SyncSetSlideshowLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;

        public SyncSetSlideshowLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndSetSlideshow(aAsyncHandle);
        }
    };

    internal class SyncSetPasswordLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;

        public SyncSetPasswordLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndSetPassword(aAsyncHandle);
        }
    };

    internal class SyncDiscTypeLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private String iDiscType;

        public SyncDiscTypeLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public String DiscType()
        {
            return iDiscType;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndDiscType(aAsyncHandle, out iDiscType);
        }
    };

    internal class SyncTitleLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private int iTitle;

        public SyncTitleLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public int Title()
        {
            return iTitle;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndTitle(aAsyncHandle, out iTitle);
        }
    };

    internal class SyncTrayStateLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private String iTrayState;

        public SyncTrayStateLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public String TrayState()
        {
            return iTrayState;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndTrayState(aAsyncHandle, out iTrayState);
        }
    };

    internal class SyncDiscStateLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private String iDiscState;

        public SyncDiscStateLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public String DiscState()
        {
            return iDiscState;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndDiscState(aAsyncHandle, out iDiscState);
        }
    };

    internal class SyncPlayStateLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private String iPlayState;

        public SyncPlayStateLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public String PlayState()
        {
            return iPlayState;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndPlayState(aAsyncHandle, out iPlayState);
        }
    };

    internal class SyncSearchTypeLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private String iSearchType;

        public SyncSearchTypeLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public String SearchType()
        {
            return iSearchType;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndSearchType(aAsyncHandle, out iSearchType);
        }
    };

    internal class SyncSearchSpeedLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private int iSearchSpeed;

        public SyncSearchSpeedLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public int SearchSpeed()
        {
            return iSearchSpeed;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndSearchSpeed(aAsyncHandle, out iSearchSpeed);
        }
    };

    internal class SyncTrackLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private int iTrack;

        public SyncTrackLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public int Track()
        {
            return iTrack;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndTrack(aAsyncHandle, out iTrack);
        }
    };

    internal class SyncTrackElapsedTimeLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private String iTrackElapsedTime;

        public SyncTrackElapsedTimeLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public String TrackElapsedTime()
        {
            return iTrackElapsedTime;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndTrackElapsedTime(aAsyncHandle, out iTrackElapsedTime);
        }
    };

    internal class SyncTrackRemainingTimeLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private String iTrackRemainingTime;

        public SyncTrackRemainingTimeLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public String TrackRemainingTime()
        {
            return iTrackRemainingTime;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndTrackRemainingTime(aAsyncHandle, out iTrackRemainingTime);
        }
    };

    internal class SyncDiscElapsedTimeLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private String iDiscElapsedTime;

        public SyncDiscElapsedTimeLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public String DiscElapsedTime()
        {
            return iDiscElapsedTime;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndDiscElapsedTime(aAsyncHandle, out iDiscElapsedTime);
        }
    };

    internal class SyncDiscRemainingTimeLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private String iDiscRemainingTime;

        public SyncDiscRemainingTimeLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public String DiscRemainingTime()
        {
            return iDiscRemainingTime;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndDiscRemainingTime(aAsyncHandle, out iDiscRemainingTime);
        }
    };

    internal class SyncRepeatModeLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private String iRepeatMode;

        public SyncRepeatModeLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public String RepeatMode()
        {
            return iRepeatMode;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndRepeatMode(aAsyncHandle, out iRepeatMode);
        }
    };

    internal class SyncIntroModeLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private bool iIntroMode;

        public SyncIntroModeLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public bool IntroMode()
        {
            return iIntroMode;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndIntroMode(aAsyncHandle, out iIntroMode);
        }
    };

    internal class SyncProgramModeLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private String iProgramMode;

        public SyncProgramModeLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public String ProgramMode()
        {
            return iProgramMode;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndProgramMode(aAsyncHandle, out iProgramMode);
        }
    };

    internal class SyncDomainLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private String iDomain;

        public SyncDomainLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public String Domain()
        {
            return iDomain;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndDomain(aAsyncHandle, out iDomain);
        }
    };

    internal class SyncAngleLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private int iAngle;

        public SyncAngleLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public int Angle()
        {
            return iAngle;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndAngle(aAsyncHandle, out iAngle);
        }
    };

    internal class SyncTotalAnglesLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private int iTotalAngles;

        public SyncTotalAnglesLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public int TotalAngles()
        {
            return iTotalAngles;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndTotalAngles(aAsyncHandle, out iTotalAngles);
        }
    };

    internal class SyncSubtitleLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private int iSubtitle;

        public SyncSubtitleLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public int Subtitle()
        {
            return iSubtitle;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndSubtitle(aAsyncHandle, out iSubtitle);
        }
    };

    internal class SyncAudioTrackLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private int iAudioTrack;

        public SyncAudioTrackLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public int AudioTrack()
        {
            return iAudioTrack;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndAudioTrack(aAsyncHandle, out iAudioTrack);
        }
    };

    internal class SyncZoomLevelLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private String iZoomLevel;

        public SyncZoomLevelLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public String ZoomLevel()
        {
            return iZoomLevel;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndZoomLevel(aAsyncHandle, out iZoomLevel);
        }
    };

    internal class SyncSetupModeLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private bool iSetupMode;

        public SyncSetupModeLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public bool SetupMode()
        {
            return iSetupMode;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndSetupMode(aAsyncHandle, out iSetupMode);
        }
    };

    internal class SyncSacdStateLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private String iSacdState;

        public SyncSacdStateLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public String SacdState()
        {
            return iSacdState;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndSacdState(aAsyncHandle, out iSacdState);
        }
    };

    internal class SyncSlideshowLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private String iSlideshow;

        public SyncSlideshowLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public String Slideshow()
        {
            return iSlideshow;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndSlideshow(aAsyncHandle, out iSlideshow);
        }
    };

    internal class SyncErrorLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private String iError;

        public SyncErrorLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public String Error()
        {
            return iError;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndError(aAsyncHandle, out iError);
        }
    };

    internal class SyncOrientationLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private String iOrientation;

        public SyncOrientationLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public String Orientation()
        {
            return iOrientation;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndOrientation(aAsyncHandle, out iOrientation);
        }
    };

    internal class SyncDiscLengthLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private String iDiscLength;

        public SyncDiscLengthLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public String DiscLength()
        {
            return iDiscLength;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndDiscLength(aAsyncHandle, out iDiscLength);
        }
    };

    internal class SyncTrackLengthLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private String iTrackLength;

        public SyncTrackLengthLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public String TrackLength()
        {
            return iTrackLength;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndTrackLength(aAsyncHandle, out iTrackLength);
        }
    };

    internal class SyncTotalTracksLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private int iTotalTracks;

        public SyncTotalTracksLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public int TotalTracks()
        {
            return iTotalTracks;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndTotalTracks(aAsyncHandle, out iTotalTracks);
        }
    };

    internal class SyncTotalTitlesLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private int iTotalTitles;

        public SyncTotalTitlesLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public int TotalTitles()
        {
            return iTotalTitles;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndTotalTitles(aAsyncHandle, out iTotalTitles);
        }
    };

    internal class SyncGenreLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private String iGenre;

        public SyncGenreLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public String Genre()
        {
            return iGenre;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndGenre(aAsyncHandle, out iGenre);
        }
    };

    internal class SyncEncodingLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private uint iEncoding;

        public SyncEncodingLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public uint Encoding()
        {
            return iEncoding;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndEncoding(aAsyncHandle, out iEncoding);
        }
    };

    internal class SyncFileSizeLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private uint iFileSize;

        public SyncFileSizeLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public uint FileSize()
        {
            return iFileSize;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndFileSize(aAsyncHandle, out iFileSize);
        }
    };

    internal class SyncDiscIdLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private uint iDiscId;

        public SyncDiscIdLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public uint DiscId()
        {
            return iDiscId;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndDiscId(aAsyncHandle, out iDiscId);
        }
    };

    internal class SyncYearLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private String iYear;

        public SyncYearLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public String Year()
        {
            return iYear;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndYear(aAsyncHandle, out iYear);
        }
    };

    internal class SyncTrackNameLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private String iTrackName;

        public SyncTrackNameLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public String TrackName()
        {
            return iTrackName;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndTrackName(aAsyncHandle, out iTrackName);
        }
    };

    internal class SyncArtistNameLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private String iArtistName;

        public SyncArtistNameLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public String ArtistName()
        {
            return iArtistName;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndArtistName(aAsyncHandle, out iArtistName);
        }
    };

    internal class SyncAlbumNameLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private String iAlbumName;

        public SyncAlbumNameLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public String AlbumName()
        {
            return iAlbumName;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndAlbumName(aAsyncHandle, out iAlbumName);
        }
    };

    internal class SyncCommentLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private String iComment;

        public SyncCommentLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public String Comment()
        {
            return iComment;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndComment(aAsyncHandle, out iComment);
        }
    };

    internal class SyncFileNameLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private String iFileName;

        public SyncFileNameLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public String FileName()
        {
            return iFileName;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndFileName(aAsyncHandle, out iFileName);
        }
    };

    internal class SyncSystemCapabilitiesLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private byte[] iSystemCapabilities;

        public SyncSystemCapabilitiesLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public byte[] SystemCapabilities()
        {
            return iSystemCapabilities;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndSystemCapabilities(aAsyncHandle, out iSystemCapabilities);
        }
    };

    internal class SyncDiscCapabilitiesLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private byte[] iDiscCapabilities;

        public SyncDiscCapabilitiesLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public byte[] DiscCapabilities()
        {
            return iDiscCapabilities;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndDiscCapabilities(aAsyncHandle, out iDiscCapabilities);
        }
    };

    internal class SyncZoomLevelInfoLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private byte[] iZoomLevelInfo;

        public SyncZoomLevelInfoLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public byte[] ZoomLevelInfo()
        {
            return iZoomLevelInfo;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndZoomLevelInfo(aAsyncHandle, out iZoomLevelInfo);
        }
    };

    internal class SyncSubtitleInfoLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private byte[] iSubtitleInfo;

        public SyncSubtitleInfoLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public byte[] SubtitleInfo()
        {
            return iSubtitleInfo;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndSubtitleInfo(aAsyncHandle, out iSubtitleInfo);
        }
    };

    internal class SyncAudioTrackInfoLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private byte[] iAudioTrackInfo;

        public SyncAudioTrackInfoLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public byte[] AudioTrackInfo()
        {
            return iAudioTrackInfo;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndAudioTrackInfo(aAsyncHandle, out iAudioTrackInfo);
        }
    };

    internal class SyncTableOfContentsLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private byte[] iTableOfContents;

        public SyncTableOfContentsLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public byte[] TableOfContents()
        {
            return iTableOfContents;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndTableOfContents(aAsyncHandle, out iTableOfContents);
        }
    };

    internal class SyncDirectoryStructureLinnCoUkSdp1 : SyncProxyAction
    {
        private CpProxyLinnCoUkSdp1 iService;
        private byte[] iDirectoryStructure;

        public SyncDirectoryStructureLinnCoUkSdp1(CpProxyLinnCoUkSdp1 aProxy)
        {
            iService = aProxy;
        }
        public byte[] DirectoryStructure()
        {
            return iDirectoryStructure;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndDirectoryStructure(aAsyncHandle, out iDirectoryStructure);
        }
    };

    /// <summary>
    /// Proxy for the linn.co.uk:Sdp:1 UPnP service
    /// </summary>
    public class CpProxyLinnCoUkSdp1 : CpProxy, IDisposable, ICpProxyLinnCoUkSdp1
    {
        private OpenHome.Net.Core.Action iActionOpen;
        private OpenHome.Net.Core.Action iActionClose;
        private OpenHome.Net.Core.Action iActionPlay;
        private OpenHome.Net.Core.Action iActionStop;
        private OpenHome.Net.Core.Action iActionPause;
        private OpenHome.Net.Core.Action iActionResume;
        private OpenHome.Net.Core.Action iActionSearch;
        private OpenHome.Net.Core.Action iActionSetTrack;
        private OpenHome.Net.Core.Action iActionSetTime;
        private OpenHome.Net.Core.Action iActionSetProgramOff;
        private OpenHome.Net.Core.Action iActionSetProgramInclude;
        private OpenHome.Net.Core.Action iActionSetProgramExclude;
        private OpenHome.Net.Core.Action iActionSetProgramRandom;
        private OpenHome.Net.Core.Action iActionSetProgramShuffle;
        private OpenHome.Net.Core.Action iActionSetRepeatOff;
        private OpenHome.Net.Core.Action iActionSetRepeatAll;
        private OpenHome.Net.Core.Action iActionSetRepeatTrack;
        private OpenHome.Net.Core.Action iActionSetRepeatAb;
        private OpenHome.Net.Core.Action iActionSetIntroMode;
        private OpenHome.Net.Core.Action iActionSetNext;
        private OpenHome.Net.Core.Action iActionSetPrev;
        private OpenHome.Net.Core.Action iActionRootMenu;
        private OpenHome.Net.Core.Action iActionTitleMenu;
        private OpenHome.Net.Core.Action iActionSetSetupMode;
        private OpenHome.Net.Core.Action iActionSetAngle;
        private OpenHome.Net.Core.Action iActionSetAudioTrack;
        private OpenHome.Net.Core.Action iActionSetSubtitle;
        private OpenHome.Net.Core.Action iActionSetZoom;
        private OpenHome.Net.Core.Action iActionSetSacdLayer;
        private OpenHome.Net.Core.Action iActionNavigate;
        private OpenHome.Net.Core.Action iActionSetSlideshow;
        private OpenHome.Net.Core.Action iActionSetPassword;
        private OpenHome.Net.Core.Action iActionDiscType;
        private OpenHome.Net.Core.Action iActionTitle;
        private OpenHome.Net.Core.Action iActionTrayState;
        private OpenHome.Net.Core.Action iActionDiscState;
        private OpenHome.Net.Core.Action iActionPlayState;
        private OpenHome.Net.Core.Action iActionSearchType;
        private OpenHome.Net.Core.Action iActionSearchSpeed;
        private OpenHome.Net.Core.Action iActionTrack;
        private OpenHome.Net.Core.Action iActionTrackElapsedTime;
        private OpenHome.Net.Core.Action iActionTrackRemainingTime;
        private OpenHome.Net.Core.Action iActionDiscElapsedTime;
        private OpenHome.Net.Core.Action iActionDiscRemainingTime;
        private OpenHome.Net.Core.Action iActionRepeatMode;
        private OpenHome.Net.Core.Action iActionIntroMode;
        private OpenHome.Net.Core.Action iActionProgramMode;
        private OpenHome.Net.Core.Action iActionDomain;
        private OpenHome.Net.Core.Action iActionAngle;
        private OpenHome.Net.Core.Action iActionTotalAngles;
        private OpenHome.Net.Core.Action iActionSubtitle;
        private OpenHome.Net.Core.Action iActionAudioTrack;
        private OpenHome.Net.Core.Action iActionZoomLevel;
        private OpenHome.Net.Core.Action iActionSetupMode;
        private OpenHome.Net.Core.Action iActionSacdState;
        private OpenHome.Net.Core.Action iActionSlideshow;
        private OpenHome.Net.Core.Action iActionError;
        private OpenHome.Net.Core.Action iActionOrientation;
        private OpenHome.Net.Core.Action iActionDiscLength;
        private OpenHome.Net.Core.Action iActionTrackLength;
        private OpenHome.Net.Core.Action iActionTotalTracks;
        private OpenHome.Net.Core.Action iActionTotalTitles;
        private OpenHome.Net.Core.Action iActionGenre;
        private OpenHome.Net.Core.Action iActionEncoding;
        private OpenHome.Net.Core.Action iActionFileSize;
        private OpenHome.Net.Core.Action iActionDiscId;
        private OpenHome.Net.Core.Action iActionYear;
        private OpenHome.Net.Core.Action iActionTrackName;
        private OpenHome.Net.Core.Action iActionArtistName;
        private OpenHome.Net.Core.Action iActionAlbumName;
        private OpenHome.Net.Core.Action iActionComment;
        private OpenHome.Net.Core.Action iActionFileName;
        private OpenHome.Net.Core.Action iActionSystemCapabilities;
        private OpenHome.Net.Core.Action iActionDiscCapabilities;
        private OpenHome.Net.Core.Action iActionZoomLevelInfo;
        private OpenHome.Net.Core.Action iActionSubtitleInfo;
        private OpenHome.Net.Core.Action iActionAudioTrackInfo;
        private OpenHome.Net.Core.Action iActionTableOfContents;
        private OpenHome.Net.Core.Action iActionDirectoryStructure;
        private PropertyString iDiscType;
        private PropertyInt iTitle;
        private PropertyString iTrayState;
        private PropertyString iDiscState;
        private PropertyString iPlayState;
        private PropertyString iSearchType;
        private PropertyInt iSearchSpeed;
        private PropertyInt iTrack;
        private PropertyString iRepeatMode;
        private PropertyBool iIntroMode;
        private PropertyString iProgramMode;
        private PropertyString iDomain;
        private PropertyInt iAngle;
        private PropertyInt iTotalAngles;
        private PropertyInt iSubtitle;
        private PropertyInt iAudioTrack;
        private PropertyString iZoomLevel;
        private PropertyBool iSetupMode;
        private PropertyString iSacdState;
        private PropertyString iSlideshow;
        private PropertyString iError;
        private PropertyString iOrientation;
        private PropertyInt iTotalTracks;
        private PropertyInt iTotalTitles;
        private PropertyUint iEncoding;
        private PropertyUint iFileSize;
        private PropertyUint iDiscId;
        private PropertyString iDiscLength;
        private PropertyString iTrackLength;
        private PropertyString iGenre;
        private PropertyString iYear;
        private PropertyString iTrackName;
        private PropertyString iArtistName;
        private PropertyString iAlbumName;
        private PropertyString iComment;
        private PropertyBinary iFileName;
        private PropertyBinary iSystemCapabilities;
        private PropertyBinary iDiscCapabilities;
        private PropertyBinary iZoomLevelInfo;
        private PropertyBinary iSubtitleInfo;
        private PropertyBinary iAudioTrackInfo;
        private PropertyBinary iTableOfContents;
        private PropertyBinary iDirectoryStructure;
        private System.Action iDiscTypeChanged;
        private System.Action iTitleChanged;
        private System.Action iTrayStateChanged;
        private System.Action iDiscStateChanged;
        private System.Action iPlayStateChanged;
        private System.Action iSearchTypeChanged;
        private System.Action iSearchSpeedChanged;
        private System.Action iTrackChanged;
        private System.Action iRepeatModeChanged;
        private System.Action iIntroModeChanged;
        private System.Action iProgramModeChanged;
        private System.Action iDomainChanged;
        private System.Action iAngleChanged;
        private System.Action iTotalAnglesChanged;
        private System.Action iSubtitleChanged;
        private System.Action iAudioTrackChanged;
        private System.Action iZoomLevelChanged;
        private System.Action iSetupModeChanged;
        private System.Action iSacdStateChanged;
        private System.Action iSlideshowChanged;
        private System.Action iErrorChanged;
        private System.Action iOrientationChanged;
        private System.Action iTotalTracksChanged;
        private System.Action iTotalTitlesChanged;
        private System.Action iEncodingChanged;
        private System.Action iFileSizeChanged;
        private System.Action iDiscIdChanged;
        private System.Action iDiscLengthChanged;
        private System.Action iTrackLengthChanged;
        private System.Action iGenreChanged;
        private System.Action iYearChanged;
        private System.Action iTrackNameChanged;
        private System.Action iArtistNameChanged;
        private System.Action iAlbumNameChanged;
        private System.Action iCommentChanged;
        private System.Action iFileNameChanged;
        private System.Action iSystemCapabilitiesChanged;
        private System.Action iDiscCapabilitiesChanged;
        private System.Action iZoomLevelInfoChanged;
        private System.Action iSubtitleInfoChanged;
        private System.Action iAudioTrackInfoChanged;
        private System.Action iTableOfContentsChanged;
        private System.Action iDirectoryStructureChanged;
        private Mutex iPropertyLock;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>Use CpProxy::[Un]Subscribe() to enable/disable querying of state variable and reporting of their changes.</remarks>
        /// <param name="aDevice">The device to use</param>
        public CpProxyLinnCoUkSdp1(CpDevice aDevice)
            : base("linn-co-uk", "Sdp", 1, aDevice)
        {
            OpenHome.Net.Core.Parameter param;
            List<String> allowedValues = new List<String>();

            iActionOpen = new OpenHome.Net.Core.Action("Open");

            iActionClose = new OpenHome.Net.Core.Action("Close");

            iActionPlay = new OpenHome.Net.Core.Action("Play");

            iActionStop = new OpenHome.Net.Core.Action("Stop");

            iActionPause = new OpenHome.Net.Core.Action("Pause");

            iActionResume = new OpenHome.Net.Core.Action("Resume");

            iActionSearch = new OpenHome.Net.Core.Action("Search");
            allowedValues.Add("None");
            allowedValues.Add("Fast Forward");
            allowedValues.Add("Fast Reverse");
            allowedValues.Add("Slow Forward");
            allowedValues.Add("Slow Reverse");
            param = new ParameterString("aSearchType", allowedValues);
            iActionSearch.AddInputParameter(param);
            allowedValues.Clear();
            param = new ParameterInt("aSearchSpeed");
            iActionSearch.AddInputParameter(param);

            iActionSetTrack = new OpenHome.Net.Core.Action("SetTrack");
            param = new ParameterInt("aTrack");
            iActionSetTrack.AddInputParameter(param);
            param = new ParameterInt("aTitle");
            iActionSetTrack.AddInputParameter(param);

            iActionSetTime = new OpenHome.Net.Core.Action("SetTime");
            param = new ParameterString("aTime", allowedValues);
            iActionSetTime.AddInputParameter(param);
            param = new ParameterInt("aTitle");
            iActionSetTime.AddInputParameter(param);

            iActionSetProgramOff = new OpenHome.Net.Core.Action("SetProgramOff");

            iActionSetProgramInclude = new OpenHome.Net.Core.Action("SetProgramInclude");
            param = new ParameterBinary("aIncludeList");
            iActionSetProgramInclude.AddInputParameter(param);

            iActionSetProgramExclude = new OpenHome.Net.Core.Action("SetProgramExclude");
            param = new ParameterBinary("aExcludeList");
            iActionSetProgramExclude.AddInputParameter(param);

            iActionSetProgramRandom = new OpenHome.Net.Core.Action("SetProgramRandom");

            iActionSetProgramShuffle = new OpenHome.Net.Core.Action("SetProgramShuffle");

            iActionSetRepeatOff = new OpenHome.Net.Core.Action("SetRepeatOff");

            iActionSetRepeatAll = new OpenHome.Net.Core.Action("SetRepeatAll");

            iActionSetRepeatTrack = new OpenHome.Net.Core.Action("SetRepeatTrack");

            iActionSetRepeatAb = new OpenHome.Net.Core.Action("SetRepeatAb");
            param = new ParameterString("aStartTime", allowedValues);
            iActionSetRepeatAb.AddInputParameter(param);
            param = new ParameterString("aEndTime", allowedValues);
            iActionSetRepeatAb.AddInputParameter(param);

            iActionSetIntroMode = new OpenHome.Net.Core.Action("SetIntroMode");
            param = new ParameterBool("aIntroMode");
            iActionSetIntroMode.AddInputParameter(param);
            param = new ParameterInt("aOffset");
            iActionSetIntroMode.AddInputParameter(param);
            param = new ParameterInt("aSeconds");
            iActionSetIntroMode.AddInputParameter(param);

            iActionSetNext = new OpenHome.Net.Core.Action("SetNext");
            allowedValues.Add("SkipTrack");
            allowedValues.Add("SkipFrame");
            allowedValues.Add("SkipSearchSpeed");
            allowedValues.Add("SkipFile");
            allowedValues.Add("SkipDisc");
            allowedValues.Add("SkipSacdLayer");
            param = new ParameterString("aSkip", allowedValues);
            iActionSetNext.AddInputParameter(param);
            allowedValues.Clear();

            iActionSetPrev = new OpenHome.Net.Core.Action("SetPrev");
            allowedValues.Add("SkipTrack");
            allowedValues.Add("SkipFrame");
            allowedValues.Add("SkipSearchSpeed");
            allowedValues.Add("SkipFile");
            allowedValues.Add("SkipDisc");
            allowedValues.Add("SkipSacdLayer");
            param = new ParameterString("aSkip", allowedValues);
            iActionSetPrev.AddInputParameter(param);
            allowedValues.Clear();

            iActionRootMenu = new OpenHome.Net.Core.Action("RootMenu");

            iActionTitleMenu = new OpenHome.Net.Core.Action("TitleMenu");

            iActionSetSetupMode = new OpenHome.Net.Core.Action("SetSetupMode");
            param = new ParameterBool("aSetupMode");
            iActionSetSetupMode.AddInputParameter(param);

            iActionSetAngle = new OpenHome.Net.Core.Action("SetAngle");
            allowedValues.Add("SelectDefault");
            allowedValues.Add("SelectNext");
            allowedValues.Add("SelectPrev");
            allowedValues.Add("SelectIndex");
            param = new ParameterString("aSelect", allowedValues);
            iActionSetAngle.AddInputParameter(param);
            allowedValues.Clear();
            param = new ParameterInt("aIndex");
            iActionSetAngle.AddInputParameter(param);

            iActionSetAudioTrack = new OpenHome.Net.Core.Action("SetAudioTrack");
            allowedValues.Add("SelectDefault");
            allowedValues.Add("SelectNext");
            allowedValues.Add("SelectPrev");
            allowedValues.Add("SelectIndex");
            param = new ParameterString("aSelect", allowedValues);
            iActionSetAudioTrack.AddInputParameter(param);
            allowedValues.Clear();
            param = new ParameterInt("aIndex");
            iActionSetAudioTrack.AddInputParameter(param);

            iActionSetSubtitle = new OpenHome.Net.Core.Action("SetSubtitle");
            allowedValues.Add("SelectDefault");
            allowedValues.Add("SelectNext");
            allowedValues.Add("SelectPrev");
            allowedValues.Add("SelectIndex");
            param = new ParameterString("aSelect", allowedValues);
            iActionSetSubtitle.AddInputParameter(param);
            allowedValues.Clear();
            param = new ParameterInt("aIndex");
            iActionSetSubtitle.AddInputParameter(param);

            iActionSetZoom = new OpenHome.Net.Core.Action("SetZoom");
            allowedValues.Add("SelectDefault");
            allowedValues.Add("SelectNext");
            allowedValues.Add("SelectPrev");
            allowedValues.Add("SelectIndex");
            param = new ParameterString("aSelect", allowedValues);
            iActionSetZoom.AddInputParameter(param);
            allowedValues.Clear();
            param = new ParameterInt("aIndex");
            iActionSetZoom.AddInputParameter(param);

            iActionSetSacdLayer = new OpenHome.Net.Core.Action("SetSacdLayer");
            allowedValues.Add("Unknown");
            allowedValues.Add("DSD Multi Channel");
            allowedValues.Add("DSD Stereo");
            allowedValues.Add("CDDA Stereo");
            allowedValues.Add("Selecting");
            param = new ParameterString("aSacdLayer", allowedValues);
            iActionSetSacdLayer.AddInputParameter(param);
            allowedValues.Clear();

            iActionNavigate = new OpenHome.Net.Core.Action("Navigate");
            allowedValues.Add("Up");
            allowedValues.Add("Down");
            allowedValues.Add("Left");
            allowedValues.Add("Right");
            allowedValues.Add("Select");
            allowedValues.Add("Return");
            param = new ParameterString("aNavigation", allowedValues);
            iActionNavigate.AddInputParameter(param);
            allowedValues.Clear();
            param = new ParameterInt("aIndex");
            iActionNavigate.AddInputParameter(param);

            iActionSetSlideshow = new OpenHome.Net.Core.Action("SetSlideshow");
            allowedValues.Add("Off");
            allowedValues.Add("Slide Show");
            allowedValues.Add("Slide Show (Photo Per Track)");
            allowedValues.Add("New Wipe Mode");
            allowedValues.Add("Thumbnails");
            param = new ParameterString("aSlideshow", allowedValues);
            iActionSetSlideshow.AddInputParameter(param);
            allowedValues.Clear();

            iActionSetPassword = new OpenHome.Net.Core.Action("SetPassword");
            param = new ParameterString("aPassword", allowedValues);
            iActionSetPassword.AddInputParameter(param);

            iActionDiscType = new OpenHome.Net.Core.Action("DiscType");
            allowedValues.Add("Unsupported Disc Type");
            allowedValues.Add("No Disc");
            allowedValues.Add("CD");
            allowedValues.Add("VCD");
            allowedValues.Add("SVCD");
            allowedValues.Add("SACD");
            allowedValues.Add("DVD");
            allowedValues.Add("DVD-Audio");
            allowedValues.Add("Data Disc");
            allowedValues.Add("Unknown");
            param = new ParameterString("aDiscType", allowedValues);
            iActionDiscType.AddOutputParameter(param);
            allowedValues.Clear();

            iActionTitle = new OpenHome.Net.Core.Action("Title");
            param = new ParameterInt("aTitle");
            iActionTitle.AddOutputParameter(param);

            iActionTrayState = new OpenHome.Net.Core.Action("TrayState");
            allowedValues.Add("Tray Open");
            allowedValues.Add("Tray Closed");
            allowedValues.Add("Tray Opening");
            allowedValues.Add("Tray Closing");
            allowedValues.Add("Unknown");
            param = new ParameterString("aTrayState", allowedValues);
            iActionTrayState.AddOutputParameter(param);
            allowedValues.Clear();

            iActionDiscState = new OpenHome.Net.Core.Action("DiscState");
            allowedValues.Add("Disc Loading");
            allowedValues.Add("Disc Loaded");
            allowedValues.Add("Unknown");
            param = new ParameterString("aDiscState", allowedValues);
            iActionDiscState.AddOutputParameter(param);
            allowedValues.Clear();

            iActionPlayState = new OpenHome.Net.Core.Action("PlayState");
            allowedValues.Add("Playing");
            allowedValues.Add("Stopped");
            allowedValues.Add("Paused");
            allowedValues.Add("Suspended");
            allowedValues.Add("Unknown");
            param = new ParameterString("aPlayState", allowedValues);
            iActionPlayState.AddOutputParameter(param);
            allowedValues.Clear();

            iActionSearchType = new OpenHome.Net.Core.Action("SearchType");
            allowedValues.Add("None");
            allowedValues.Add("Fast Forward");
            allowedValues.Add("Fast Reverse");
            allowedValues.Add("Slow Forward");
            allowedValues.Add("Slow Reverse");
            param = new ParameterString("aSearchType", allowedValues);
            iActionSearchType.AddOutputParameter(param);
            allowedValues.Clear();

            iActionSearchSpeed = new OpenHome.Net.Core.Action("SearchSpeed");
            param = new ParameterInt("aSearchSpeed");
            iActionSearchSpeed.AddOutputParameter(param);

            iActionTrack = new OpenHome.Net.Core.Action("Track");
            param = new ParameterInt("aTrack");
            iActionTrack.AddOutputParameter(param);

            iActionTrackElapsedTime = new OpenHome.Net.Core.Action("TrackElapsedTime");
            param = new ParameterString("aTrackElapsedTime", allowedValues);
            iActionTrackElapsedTime.AddOutputParameter(param);

            iActionTrackRemainingTime = new OpenHome.Net.Core.Action("TrackRemainingTime");
            param = new ParameterString("aTrackRemainingTime", allowedValues);
            iActionTrackRemainingTime.AddOutputParameter(param);

            iActionDiscElapsedTime = new OpenHome.Net.Core.Action("DiscElapsedTime");
            param = new ParameterString("aDiscElapsedTime", allowedValues);
            iActionDiscElapsedTime.AddOutputParameter(param);

            iActionDiscRemainingTime = new OpenHome.Net.Core.Action("DiscRemainingTime");
            param = new ParameterString("aDiscRemainingTime", allowedValues);
            iActionDiscRemainingTime.AddOutputParameter(param);

            iActionRepeatMode = new OpenHome.Net.Core.Action("RepeatMode");
            allowedValues.Add("Off");
            allowedValues.Add("All");
            allowedValues.Add("Track");
            allowedValues.Add("A-B");
            param = new ParameterString("aRepeatMode", allowedValues);
            iActionRepeatMode.AddOutputParameter(param);
            allowedValues.Clear();

            iActionIntroMode = new OpenHome.Net.Core.Action("IntroMode");
            param = new ParameterBool("aIntroMode");
            iActionIntroMode.AddOutputParameter(param);

            iActionProgramMode = new OpenHome.Net.Core.Action("ProgramMode");
            allowedValues.Add("Off");
            allowedValues.Add("Random");
            allowedValues.Add("Shuffle");
            allowedValues.Add("Stored");
            param = new ParameterString("aProgramMode", allowedValues);
            iActionProgramMode.AddOutputParameter(param);
            allowedValues.Clear();

            iActionDomain = new OpenHome.Net.Core.Action("Domain");
            allowedValues.Add("None");
            allowedValues.Add("Root Menu");
            allowedValues.Add("Title Menu");
            allowedValues.Add("Copyright");
            allowedValues.Add("Password");
            param = new ParameterString("aDomain", allowedValues);
            iActionDomain.AddOutputParameter(param);
            allowedValues.Clear();

            iActionAngle = new OpenHome.Net.Core.Action("Angle");
            param = new ParameterInt("aAngle");
            iActionAngle.AddOutputParameter(param);

            iActionTotalAngles = new OpenHome.Net.Core.Action("TotalAngles");
            param = new ParameterInt("aTotalAngles");
            iActionTotalAngles.AddOutputParameter(param);

            iActionSubtitle = new OpenHome.Net.Core.Action("Subtitle");
            param = new ParameterInt("aSubtitle");
            iActionSubtitle.AddOutputParameter(param);

            iActionAudioTrack = new OpenHome.Net.Core.Action("AudioTrack");
            param = new ParameterInt("aAudioTrack");
            iActionAudioTrack.AddOutputParameter(param);

            iActionZoomLevel = new OpenHome.Net.Core.Action("ZoomLevel");
            allowedValues.Add("25");
            allowedValues.Add("50");
            allowedValues.Add("100");
            allowedValues.Add("150");
            allowedValues.Add("200");
            allowedValues.Add("300");
            param = new ParameterString("aZoomLevel", allowedValues);
            iActionZoomLevel.AddOutputParameter(param);
            allowedValues.Clear();

            iActionSetupMode = new OpenHome.Net.Core.Action("SetupMode");
            param = new ParameterBool("aSetupMode");
            iActionSetupMode.AddOutputParameter(param);

            iActionSacdState = new OpenHome.Net.Core.Action("SacdState");
            allowedValues.Add("Unknown");
            allowedValues.Add("DSD Multi Channel");
            allowedValues.Add("DSD Stereo");
            allowedValues.Add("CDDA Stereo");
            allowedValues.Add("Selecting");
            param = new ParameterString("aSacdState", allowedValues);
            iActionSacdState.AddOutputParameter(param);
            allowedValues.Clear();

            iActionSlideshow = new OpenHome.Net.Core.Action("Slideshow");
            allowedValues.Add("Off");
            allowedValues.Add("Slide Show");
            allowedValues.Add("Slide Show (Photo Per Track)");
            allowedValues.Add("New Wipe Mode");
            allowedValues.Add("Thumbnails");
            param = new ParameterString("aSlideshow", allowedValues);
            iActionSlideshow.AddOutputParameter(param);
            allowedValues.Clear();

            iActionError = new OpenHome.Net.Core.Action("Error");
            allowedValues.Add("None");
            allowedValues.Add("Track out of Range");
            allowedValues.Add("Title out of Range");
            allowedValues.Add("Time out of Range");
            allowedValues.Add("Resume Not Available");
            allowedValues.Add("Program Size Invalid");
            allowedValues.Add("Domain Not Available");
            allowedValues.Add("Search Type Not Available");
            allowedValues.Add("Program Type Not Available");
            allowedValues.Add("Repeat Type Not Available");
            allowedValues.Add("Intro Mode Not Available");
            allowedValues.Add("Skip Type Not Available");
            allowedValues.Add("Setup Mode Not Available");
            allowedValues.Add("Angle Not Available");
            allowedValues.Add("Subtitle Not Available");
            allowedValues.Add("Zoom Level Not Available");
            allowedValues.Add("Audio Track Not Available");
            allowedValues.Add("Sacd Layer Not Available");
            allowedValues.Add("Invalid Navigation Request");
            allowedValues.Add("Slide Show Type Not Available");
            allowedValues.Add("Request Not Supported");
            allowedValues.Add("Table Of Contents Missing");
            allowedValues.Add("Invalid Password");
            param = new ParameterString("aError", allowedValues);
            iActionError.AddOutputParameter(param);
            allowedValues.Clear();

            iActionOrientation = new OpenHome.Net.Core.Action("Orientation");
            allowedValues.Add("Unknown");
            allowedValues.Add("0 Degrees");
            allowedValues.Add("0 Degrees (Y Mirror)");
            allowedValues.Add("90 Degrees");
            allowedValues.Add("90 Degrees (Y Mirror)");
            allowedValues.Add("180 Degrees");
            allowedValues.Add("180 Degrees (Y Mirror)");
            allowedValues.Add("270 Degrees");
            allowedValues.Add("270 Degrees (Y Mirror)");
            param = new ParameterString("aOrientation", allowedValues);
            iActionOrientation.AddOutputParameter(param);
            allowedValues.Clear();

            iActionDiscLength = new OpenHome.Net.Core.Action("DiscLength");
            param = new ParameterString("aDiscLength", allowedValues);
            iActionDiscLength.AddOutputParameter(param);

            iActionTrackLength = new OpenHome.Net.Core.Action("TrackLength");
            param = new ParameterString("aTrackLength", allowedValues);
            iActionTrackLength.AddOutputParameter(param);

            iActionTotalTracks = new OpenHome.Net.Core.Action("TotalTracks");
            param = new ParameterInt("aTotalTracks");
            iActionTotalTracks.AddOutputParameter(param);

            iActionTotalTitles = new OpenHome.Net.Core.Action("TotalTitles");
            param = new ParameterInt("aTotalTitles");
            iActionTotalTitles.AddOutputParameter(param);

            iActionGenre = new OpenHome.Net.Core.Action("Genre");
            param = new ParameterString("aGenre", allowedValues);
            iActionGenre.AddOutputParameter(param);

            iActionEncoding = new OpenHome.Net.Core.Action("Encoding");
            param = new ParameterUint("aEncoding");
            iActionEncoding.AddOutputParameter(param);

            iActionFileSize = new OpenHome.Net.Core.Action("FileSize");
            param = new ParameterUint("aFileSize");
            iActionFileSize.AddOutputParameter(param);

            iActionDiscId = new OpenHome.Net.Core.Action("DiscId");
            param = new ParameterUint("aDiscId");
            iActionDiscId.AddOutputParameter(param);

            iActionYear = new OpenHome.Net.Core.Action("Year");
            param = new ParameterString("aYear", allowedValues);
            iActionYear.AddOutputParameter(param);

            iActionTrackName = new OpenHome.Net.Core.Action("TrackName");
            param = new ParameterString("aTrackName", allowedValues);
            iActionTrackName.AddOutputParameter(param);

            iActionArtistName = new OpenHome.Net.Core.Action("ArtistName");
            param = new ParameterString("aArtistName", allowedValues);
            iActionArtistName.AddOutputParameter(param);

            iActionAlbumName = new OpenHome.Net.Core.Action("AlbumName");
            param = new ParameterString("aAlbumName", allowedValues);
            iActionAlbumName.AddOutputParameter(param);

            iActionComment = new OpenHome.Net.Core.Action("Comment");
            param = new ParameterString("aComment", allowedValues);
            iActionComment.AddOutputParameter(param);

            iActionFileName = new OpenHome.Net.Core.Action("FileName");
            param = new ParameterInt("aIndex");
            iActionFileName.AddInputParameter(param);
            param = new ParameterString("aFileName", allowedValues);
            iActionFileName.AddOutputParameter(param);

            iActionSystemCapabilities = new OpenHome.Net.Core.Action("SystemCapabilities");
            param = new ParameterBinary("aSystemCapabilities");
            iActionSystemCapabilities.AddOutputParameter(param);

            iActionDiscCapabilities = new OpenHome.Net.Core.Action("DiscCapabilities");
            param = new ParameterBinary("aDiscCapabilities");
            iActionDiscCapabilities.AddOutputParameter(param);

            iActionZoomLevelInfo = new OpenHome.Net.Core.Action("ZoomLevelInfo");
            param = new ParameterBinary("aZoomLevelInfo");
            iActionZoomLevelInfo.AddOutputParameter(param);

            iActionSubtitleInfo = new OpenHome.Net.Core.Action("SubtitleInfo");
            param = new ParameterBinary("aSubtitleInfo");
            iActionSubtitleInfo.AddOutputParameter(param);

            iActionAudioTrackInfo = new OpenHome.Net.Core.Action("AudioTrackInfo");
            param = new ParameterBinary("aAudioTrackInfo");
            iActionAudioTrackInfo.AddOutputParameter(param);

            iActionTableOfContents = new OpenHome.Net.Core.Action("TableOfContents");
            param = new ParameterBinary("aTableOfContents");
            iActionTableOfContents.AddOutputParameter(param);

            iActionDirectoryStructure = new OpenHome.Net.Core.Action("DirectoryStructure");
            param = new ParameterBinary("aDirectoryStructure");
            iActionDirectoryStructure.AddOutputParameter(param);

            iDiscType = new PropertyString("DiscType", DiscTypePropertyChanged);
            AddProperty(iDiscType);
            iTitle = new PropertyInt("Title", TitlePropertyChanged);
            AddProperty(iTitle);
            iTrayState = new PropertyString("TrayState", TrayStatePropertyChanged);
            AddProperty(iTrayState);
            iDiscState = new PropertyString("DiscState", DiscStatePropertyChanged);
            AddProperty(iDiscState);
            iPlayState = new PropertyString("PlayState", PlayStatePropertyChanged);
            AddProperty(iPlayState);
            iSearchType = new PropertyString("SearchType", SearchTypePropertyChanged);
            AddProperty(iSearchType);
            iSearchSpeed = new PropertyInt("SearchSpeed", SearchSpeedPropertyChanged);
            AddProperty(iSearchSpeed);
            iTrack = new PropertyInt("Track", TrackPropertyChanged);
            AddProperty(iTrack);
            iRepeatMode = new PropertyString("RepeatMode", RepeatModePropertyChanged);
            AddProperty(iRepeatMode);
            iIntroMode = new PropertyBool("IntroMode", IntroModePropertyChanged);
            AddProperty(iIntroMode);
            iProgramMode = new PropertyString("ProgramMode", ProgramModePropertyChanged);
            AddProperty(iProgramMode);
            iDomain = new PropertyString("Domain", DomainPropertyChanged);
            AddProperty(iDomain);
            iAngle = new PropertyInt("Angle", AnglePropertyChanged);
            AddProperty(iAngle);
            iTotalAngles = new PropertyInt("TotalAngles", TotalAnglesPropertyChanged);
            AddProperty(iTotalAngles);
            iSubtitle = new PropertyInt("Subtitle", SubtitlePropertyChanged);
            AddProperty(iSubtitle);
            iAudioTrack = new PropertyInt("AudioTrack", AudioTrackPropertyChanged);
            AddProperty(iAudioTrack);
            iZoomLevel = new PropertyString("ZoomLevel", ZoomLevelPropertyChanged);
            AddProperty(iZoomLevel);
            iSetupMode = new PropertyBool("SetupMode", SetupModePropertyChanged);
            AddProperty(iSetupMode);
            iSacdState = new PropertyString("SacdState", SacdStatePropertyChanged);
            AddProperty(iSacdState);
            iSlideshow = new PropertyString("Slideshow", SlideshowPropertyChanged);
            AddProperty(iSlideshow);
            iError = new PropertyString("Error", ErrorPropertyChanged);
            AddProperty(iError);
            iOrientation = new PropertyString("Orientation", OrientationPropertyChanged);
            AddProperty(iOrientation);
            iTotalTracks = new PropertyInt("TotalTracks", TotalTracksPropertyChanged);
            AddProperty(iTotalTracks);
            iTotalTitles = new PropertyInt("TotalTitles", TotalTitlesPropertyChanged);
            AddProperty(iTotalTitles);
            iEncoding = new PropertyUint("Encoding", EncodingPropertyChanged);
            AddProperty(iEncoding);
            iFileSize = new PropertyUint("FileSize", FileSizePropertyChanged);
            AddProperty(iFileSize);
            iDiscId = new PropertyUint("DiscId", DiscIdPropertyChanged);
            AddProperty(iDiscId);
            iDiscLength = new PropertyString("DiscLength", DiscLengthPropertyChanged);
            AddProperty(iDiscLength);
            iTrackLength = new PropertyString("TrackLength", TrackLengthPropertyChanged);
            AddProperty(iTrackLength);
            iGenre = new PropertyString("Genre", GenrePropertyChanged);
            AddProperty(iGenre);
            iYear = new PropertyString("Year", YearPropertyChanged);
            AddProperty(iYear);
            iTrackName = new PropertyString("TrackName", TrackNamePropertyChanged);
            AddProperty(iTrackName);
            iArtistName = new PropertyString("ArtistName", ArtistNamePropertyChanged);
            AddProperty(iArtistName);
            iAlbumName = new PropertyString("AlbumName", AlbumNamePropertyChanged);
            AddProperty(iAlbumName);
            iComment = new PropertyString("Comment", CommentPropertyChanged);
            AddProperty(iComment);
            iFileName = new PropertyBinary("FileName", FileNamePropertyChanged);
            AddProperty(iFileName);
            iSystemCapabilities = new PropertyBinary("SystemCapabilities", SystemCapabilitiesPropertyChanged);
            AddProperty(iSystemCapabilities);
            iDiscCapabilities = new PropertyBinary("DiscCapabilities", DiscCapabilitiesPropertyChanged);
            AddProperty(iDiscCapabilities);
            iZoomLevelInfo = new PropertyBinary("ZoomLevelInfo", ZoomLevelInfoPropertyChanged);
            AddProperty(iZoomLevelInfo);
            iSubtitleInfo = new PropertyBinary("SubtitleInfo", SubtitleInfoPropertyChanged);
            AddProperty(iSubtitleInfo);
            iAudioTrackInfo = new PropertyBinary("AudioTrackInfo", AudioTrackInfoPropertyChanged);
            AddProperty(iAudioTrackInfo);
            iTableOfContents = new PropertyBinary("TableOfContents", TableOfContentsPropertyChanged);
            AddProperty(iTableOfContents);
            iDirectoryStructure = new PropertyBinary("DirectoryStructure", DirectoryStructurePropertyChanged);
            AddProperty(iDirectoryStructure);
            
            iPropertyLock = new Mutex();
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        public void SyncOpen()
        {
            SyncOpenLinnCoUkSdp1 sync = new SyncOpenLinnCoUkSdp1(this);
            BeginOpen(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndOpen().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginOpen(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionOpen, aCallback);
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        public void EndOpen(IntPtr aAsyncHandle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        public void SyncClose()
        {
            SyncCloseLinnCoUkSdp1 sync = new SyncCloseLinnCoUkSdp1(this);
            BeginClose(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndClose().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginClose(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionClose, aCallback);
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        public void EndClose(IntPtr aAsyncHandle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        public void SyncPlay()
        {
            SyncPlayLinnCoUkSdp1 sync = new SyncPlayLinnCoUkSdp1(this);
            BeginPlay(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndPlay().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginPlay(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionPlay, aCallback);
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        public void EndPlay(IntPtr aAsyncHandle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        public void SyncStop()
        {
            SyncStopLinnCoUkSdp1 sync = new SyncStopLinnCoUkSdp1(this);
            BeginStop(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndStop().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginStop(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionStop, aCallback);
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        public void EndStop(IntPtr aAsyncHandle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        public void SyncPause()
        {
            SyncPauseLinnCoUkSdp1 sync = new SyncPauseLinnCoUkSdp1(this);
            BeginPause(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndPause().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginPause(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionPause, aCallback);
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        public void EndPause(IntPtr aAsyncHandle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        public void SyncResume()
        {
            SyncResumeLinnCoUkSdp1 sync = new SyncResumeLinnCoUkSdp1(this);
            BeginResume(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndResume().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginResume(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionResume, aCallback);
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        public void EndResume(IntPtr aAsyncHandle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aSearchType"></param>
        /// <param name="aSearchSpeed"></param>
        public void SyncSearch(String aSearchType, int aSearchSpeed)
        {
            SyncSearchLinnCoUkSdp1 sync = new SyncSearchLinnCoUkSdp1(this);
            BeginSearch(aSearchType, aSearchSpeed, sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndSearch().</remarks>
        /// <param name="aSearchType"></param>
        /// <param name="aSearchSpeed"></param>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginSearch(String aSearchType, int aSearchSpeed, CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionSearch, aCallback);
            int inIndex = 0;
            invocation.AddInput(new ArgumentString((ParameterString)iActionSearch.InputParameter(inIndex++), aSearchType));
            invocation.AddInput(new ArgumentInt((ParameterInt)iActionSearch.InputParameter(inIndex++), aSearchSpeed));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        public void EndSearch(IntPtr aAsyncHandle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aTrack"></param>
        /// <param name="aTitle"></param>
        public void SyncSetTrack(int aTrack, int aTitle)
        {
            SyncSetTrackLinnCoUkSdp1 sync = new SyncSetTrackLinnCoUkSdp1(this);
            BeginSetTrack(aTrack, aTitle, sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndSetTrack().</remarks>
        /// <param name="aTrack"></param>
        /// <param name="aTitle"></param>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginSetTrack(int aTrack, int aTitle, CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionSetTrack, aCallback);
            int inIndex = 0;
            invocation.AddInput(new ArgumentInt((ParameterInt)iActionSetTrack.InputParameter(inIndex++), aTrack));
            invocation.AddInput(new ArgumentInt((ParameterInt)iActionSetTrack.InputParameter(inIndex++), aTitle));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        public void EndSetTrack(IntPtr aAsyncHandle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aTime"></param>
        /// <param name="aTitle"></param>
        public void SyncSetTime(String aTime, int aTitle)
        {
            SyncSetTimeLinnCoUkSdp1 sync = new SyncSetTimeLinnCoUkSdp1(this);
            BeginSetTime(aTime, aTitle, sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndSetTime().</remarks>
        /// <param name="aTime"></param>
        /// <param name="aTitle"></param>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginSetTime(String aTime, int aTitle, CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionSetTime, aCallback);
            int inIndex = 0;
            invocation.AddInput(new ArgumentString((ParameterString)iActionSetTime.InputParameter(inIndex++), aTime));
            invocation.AddInput(new ArgumentInt((ParameterInt)iActionSetTime.InputParameter(inIndex++), aTitle));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        public void EndSetTime(IntPtr aAsyncHandle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        public void SyncSetProgramOff()
        {
            SyncSetProgramOffLinnCoUkSdp1 sync = new SyncSetProgramOffLinnCoUkSdp1(this);
            BeginSetProgramOff(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndSetProgramOff().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginSetProgramOff(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionSetProgramOff, aCallback);
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        public void EndSetProgramOff(IntPtr aAsyncHandle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aIncludeList"></param>
        public void SyncSetProgramInclude(byte[] aIncludeList)
        {
            SyncSetProgramIncludeLinnCoUkSdp1 sync = new SyncSetProgramIncludeLinnCoUkSdp1(this);
            BeginSetProgramInclude(aIncludeList, sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndSetProgramInclude().</remarks>
        /// <param name="aIncludeList"></param>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginSetProgramInclude(byte[] aIncludeList, CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionSetProgramInclude, aCallback);
            int inIndex = 0;
            invocation.AddInput(new ArgumentBinary((ParameterBinary)iActionSetProgramInclude.InputParameter(inIndex++), aIncludeList));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        public void EndSetProgramInclude(IntPtr aAsyncHandle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aExcludeList"></param>
        public void SyncSetProgramExclude(byte[] aExcludeList)
        {
            SyncSetProgramExcludeLinnCoUkSdp1 sync = new SyncSetProgramExcludeLinnCoUkSdp1(this);
            BeginSetProgramExclude(aExcludeList, sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndSetProgramExclude().</remarks>
        /// <param name="aExcludeList"></param>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginSetProgramExclude(byte[] aExcludeList, CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionSetProgramExclude, aCallback);
            int inIndex = 0;
            invocation.AddInput(new ArgumentBinary((ParameterBinary)iActionSetProgramExclude.InputParameter(inIndex++), aExcludeList));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        public void EndSetProgramExclude(IntPtr aAsyncHandle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        public void SyncSetProgramRandom()
        {
            SyncSetProgramRandomLinnCoUkSdp1 sync = new SyncSetProgramRandomLinnCoUkSdp1(this);
            BeginSetProgramRandom(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndSetProgramRandom().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginSetProgramRandom(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionSetProgramRandom, aCallback);
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        public void EndSetProgramRandom(IntPtr aAsyncHandle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        public void SyncSetProgramShuffle()
        {
            SyncSetProgramShuffleLinnCoUkSdp1 sync = new SyncSetProgramShuffleLinnCoUkSdp1(this);
            BeginSetProgramShuffle(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndSetProgramShuffle().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginSetProgramShuffle(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionSetProgramShuffle, aCallback);
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        public void EndSetProgramShuffle(IntPtr aAsyncHandle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        public void SyncSetRepeatOff()
        {
            SyncSetRepeatOffLinnCoUkSdp1 sync = new SyncSetRepeatOffLinnCoUkSdp1(this);
            BeginSetRepeatOff(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndSetRepeatOff().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginSetRepeatOff(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionSetRepeatOff, aCallback);
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        public void EndSetRepeatOff(IntPtr aAsyncHandle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        public void SyncSetRepeatAll()
        {
            SyncSetRepeatAllLinnCoUkSdp1 sync = new SyncSetRepeatAllLinnCoUkSdp1(this);
            BeginSetRepeatAll(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndSetRepeatAll().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginSetRepeatAll(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionSetRepeatAll, aCallback);
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        public void EndSetRepeatAll(IntPtr aAsyncHandle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        public void SyncSetRepeatTrack()
        {
            SyncSetRepeatTrackLinnCoUkSdp1 sync = new SyncSetRepeatTrackLinnCoUkSdp1(this);
            BeginSetRepeatTrack(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndSetRepeatTrack().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginSetRepeatTrack(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionSetRepeatTrack, aCallback);
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        public void EndSetRepeatTrack(IntPtr aAsyncHandle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aStartTime"></param>
        /// <param name="aEndTime"></param>
        public void SyncSetRepeatAb(String aStartTime, String aEndTime)
        {
            SyncSetRepeatAbLinnCoUkSdp1 sync = new SyncSetRepeatAbLinnCoUkSdp1(this);
            BeginSetRepeatAb(aStartTime, aEndTime, sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndSetRepeatAb().</remarks>
        /// <param name="aStartTime"></param>
        /// <param name="aEndTime"></param>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginSetRepeatAb(String aStartTime, String aEndTime, CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionSetRepeatAb, aCallback);
            int inIndex = 0;
            invocation.AddInput(new ArgumentString((ParameterString)iActionSetRepeatAb.InputParameter(inIndex++), aStartTime));
            invocation.AddInput(new ArgumentString((ParameterString)iActionSetRepeatAb.InputParameter(inIndex++), aEndTime));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        public void EndSetRepeatAb(IntPtr aAsyncHandle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aIntroMode"></param>
        /// <param name="aOffset"></param>
        /// <param name="aSeconds"></param>
        public void SyncSetIntroMode(bool aIntroMode, int aOffset, int aSeconds)
        {
            SyncSetIntroModeLinnCoUkSdp1 sync = new SyncSetIntroModeLinnCoUkSdp1(this);
            BeginSetIntroMode(aIntroMode, aOffset, aSeconds, sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndSetIntroMode().</remarks>
        /// <param name="aIntroMode"></param>
        /// <param name="aOffset"></param>
        /// <param name="aSeconds"></param>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginSetIntroMode(bool aIntroMode, int aOffset, int aSeconds, CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionSetIntroMode, aCallback);
            int inIndex = 0;
            invocation.AddInput(new ArgumentBool((ParameterBool)iActionSetIntroMode.InputParameter(inIndex++), aIntroMode));
            invocation.AddInput(new ArgumentInt((ParameterInt)iActionSetIntroMode.InputParameter(inIndex++), aOffset));
            invocation.AddInput(new ArgumentInt((ParameterInt)iActionSetIntroMode.InputParameter(inIndex++), aSeconds));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        public void EndSetIntroMode(IntPtr aAsyncHandle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aSkip"></param>
        public void SyncSetNext(String aSkip)
        {
            SyncSetNextLinnCoUkSdp1 sync = new SyncSetNextLinnCoUkSdp1(this);
            BeginSetNext(aSkip, sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndSetNext().</remarks>
        /// <param name="aSkip"></param>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginSetNext(String aSkip, CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionSetNext, aCallback);
            int inIndex = 0;
            invocation.AddInput(new ArgumentString((ParameterString)iActionSetNext.InputParameter(inIndex++), aSkip));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        public void EndSetNext(IntPtr aAsyncHandle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aSkip"></param>
        public void SyncSetPrev(String aSkip)
        {
            SyncSetPrevLinnCoUkSdp1 sync = new SyncSetPrevLinnCoUkSdp1(this);
            BeginSetPrev(aSkip, sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndSetPrev().</remarks>
        /// <param name="aSkip"></param>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginSetPrev(String aSkip, CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionSetPrev, aCallback);
            int inIndex = 0;
            invocation.AddInput(new ArgumentString((ParameterString)iActionSetPrev.InputParameter(inIndex++), aSkip));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        public void EndSetPrev(IntPtr aAsyncHandle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        public void SyncRootMenu()
        {
            SyncRootMenuLinnCoUkSdp1 sync = new SyncRootMenuLinnCoUkSdp1(this);
            BeginRootMenu(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndRootMenu().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginRootMenu(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionRootMenu, aCallback);
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        public void EndRootMenu(IntPtr aAsyncHandle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        public void SyncTitleMenu()
        {
            SyncTitleMenuLinnCoUkSdp1 sync = new SyncTitleMenuLinnCoUkSdp1(this);
            BeginTitleMenu(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndTitleMenu().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginTitleMenu(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionTitleMenu, aCallback);
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        public void EndTitleMenu(IntPtr aAsyncHandle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aSetupMode"></param>
        public void SyncSetSetupMode(bool aSetupMode)
        {
            SyncSetSetupModeLinnCoUkSdp1 sync = new SyncSetSetupModeLinnCoUkSdp1(this);
            BeginSetSetupMode(aSetupMode, sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndSetSetupMode().</remarks>
        /// <param name="aSetupMode"></param>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginSetSetupMode(bool aSetupMode, CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionSetSetupMode, aCallback);
            int inIndex = 0;
            invocation.AddInput(new ArgumentBool((ParameterBool)iActionSetSetupMode.InputParameter(inIndex++), aSetupMode));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        public void EndSetSetupMode(IntPtr aAsyncHandle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aSelect"></param>
        /// <param name="aIndex"></param>
        public void SyncSetAngle(String aSelect, int aIndex)
        {
            SyncSetAngleLinnCoUkSdp1 sync = new SyncSetAngleLinnCoUkSdp1(this);
            BeginSetAngle(aSelect, aIndex, sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndSetAngle().</remarks>
        /// <param name="aSelect"></param>
        /// <param name="aIndex"></param>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginSetAngle(String aSelect, int aIndex, CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionSetAngle, aCallback);
            int inIndex = 0;
            invocation.AddInput(new ArgumentString((ParameterString)iActionSetAngle.InputParameter(inIndex++), aSelect));
            invocation.AddInput(new ArgumentInt((ParameterInt)iActionSetAngle.InputParameter(inIndex++), aIndex));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        public void EndSetAngle(IntPtr aAsyncHandle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aSelect"></param>
        /// <param name="aIndex"></param>
        public void SyncSetAudioTrack(String aSelect, int aIndex)
        {
            SyncSetAudioTrackLinnCoUkSdp1 sync = new SyncSetAudioTrackLinnCoUkSdp1(this);
            BeginSetAudioTrack(aSelect, aIndex, sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndSetAudioTrack().</remarks>
        /// <param name="aSelect"></param>
        /// <param name="aIndex"></param>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginSetAudioTrack(String aSelect, int aIndex, CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionSetAudioTrack, aCallback);
            int inIndex = 0;
            invocation.AddInput(new ArgumentString((ParameterString)iActionSetAudioTrack.InputParameter(inIndex++), aSelect));
            invocation.AddInput(new ArgumentInt((ParameterInt)iActionSetAudioTrack.InputParameter(inIndex++), aIndex));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        public void EndSetAudioTrack(IntPtr aAsyncHandle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aSelect"></param>
        /// <param name="aIndex"></param>
        public void SyncSetSubtitle(String aSelect, int aIndex)
        {
            SyncSetSubtitleLinnCoUkSdp1 sync = new SyncSetSubtitleLinnCoUkSdp1(this);
            BeginSetSubtitle(aSelect, aIndex, sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndSetSubtitle().</remarks>
        /// <param name="aSelect"></param>
        /// <param name="aIndex"></param>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginSetSubtitle(String aSelect, int aIndex, CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionSetSubtitle, aCallback);
            int inIndex = 0;
            invocation.AddInput(new ArgumentString((ParameterString)iActionSetSubtitle.InputParameter(inIndex++), aSelect));
            invocation.AddInput(new ArgumentInt((ParameterInt)iActionSetSubtitle.InputParameter(inIndex++), aIndex));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        public void EndSetSubtitle(IntPtr aAsyncHandle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aSelect"></param>
        /// <param name="aIndex"></param>
        public void SyncSetZoom(String aSelect, int aIndex)
        {
            SyncSetZoomLinnCoUkSdp1 sync = new SyncSetZoomLinnCoUkSdp1(this);
            BeginSetZoom(aSelect, aIndex, sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndSetZoom().</remarks>
        /// <param name="aSelect"></param>
        /// <param name="aIndex"></param>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginSetZoom(String aSelect, int aIndex, CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionSetZoom, aCallback);
            int inIndex = 0;
            invocation.AddInput(new ArgumentString((ParameterString)iActionSetZoom.InputParameter(inIndex++), aSelect));
            invocation.AddInput(new ArgumentInt((ParameterInt)iActionSetZoom.InputParameter(inIndex++), aIndex));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        public void EndSetZoom(IntPtr aAsyncHandle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aSacdLayer"></param>
        public void SyncSetSacdLayer(String aSacdLayer)
        {
            SyncSetSacdLayerLinnCoUkSdp1 sync = new SyncSetSacdLayerLinnCoUkSdp1(this);
            BeginSetSacdLayer(aSacdLayer, sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndSetSacdLayer().</remarks>
        /// <param name="aSacdLayer"></param>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginSetSacdLayer(String aSacdLayer, CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionSetSacdLayer, aCallback);
            int inIndex = 0;
            invocation.AddInput(new ArgumentString((ParameterString)iActionSetSacdLayer.InputParameter(inIndex++), aSacdLayer));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        public void EndSetSacdLayer(IntPtr aAsyncHandle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aNavigation"></param>
        /// <param name="aIndex"></param>
        public void SyncNavigate(String aNavigation, int aIndex)
        {
            SyncNavigateLinnCoUkSdp1 sync = new SyncNavigateLinnCoUkSdp1(this);
            BeginNavigate(aNavigation, aIndex, sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndNavigate().</remarks>
        /// <param name="aNavigation"></param>
        /// <param name="aIndex"></param>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginNavigate(String aNavigation, int aIndex, CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionNavigate, aCallback);
            int inIndex = 0;
            invocation.AddInput(new ArgumentString((ParameterString)iActionNavigate.InputParameter(inIndex++), aNavigation));
            invocation.AddInput(new ArgumentInt((ParameterInt)iActionNavigate.InputParameter(inIndex++), aIndex));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        public void EndNavigate(IntPtr aAsyncHandle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aSlideshow"></param>
        public void SyncSetSlideshow(String aSlideshow)
        {
            SyncSetSlideshowLinnCoUkSdp1 sync = new SyncSetSlideshowLinnCoUkSdp1(this);
            BeginSetSlideshow(aSlideshow, sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndSetSlideshow().</remarks>
        /// <param name="aSlideshow"></param>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginSetSlideshow(String aSlideshow, CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionSetSlideshow, aCallback);
            int inIndex = 0;
            invocation.AddInput(new ArgumentString((ParameterString)iActionSetSlideshow.InputParameter(inIndex++), aSlideshow));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        public void EndSetSlideshow(IntPtr aAsyncHandle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aPassword"></param>
        public void SyncSetPassword(String aPassword)
        {
            SyncSetPasswordLinnCoUkSdp1 sync = new SyncSetPasswordLinnCoUkSdp1(this);
            BeginSetPassword(aPassword, sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndSetPassword().</remarks>
        /// <param name="aPassword"></param>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginSetPassword(String aPassword, CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionSetPassword, aCallback);
            int inIndex = 0;
            invocation.AddInput(new ArgumentString((ParameterString)iActionSetPassword.InputParameter(inIndex++), aPassword));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        public void EndSetPassword(IntPtr aAsyncHandle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aDiscType"></param>
        public void SyncDiscType(out String aDiscType)
        {
            SyncDiscTypeLinnCoUkSdp1 sync = new SyncDiscTypeLinnCoUkSdp1(this);
            BeginDiscType(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aDiscType = sync.DiscType();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndDiscType().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginDiscType(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionDiscType, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentString((ParameterString)iActionDiscType.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aDiscType"></param>
        public void EndDiscType(IntPtr aAsyncHandle, out String aDiscType)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aDiscType = Invocation.OutputString(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aTitle"></param>
        public void SyncTitle(out int aTitle)
        {
            SyncTitleLinnCoUkSdp1 sync = new SyncTitleLinnCoUkSdp1(this);
            BeginTitle(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aTitle = sync.Title();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndTitle().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginTitle(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionTitle, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentInt((ParameterInt)iActionTitle.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aTitle"></param>
        public void EndTitle(IntPtr aAsyncHandle, out int aTitle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aTitle = Invocation.OutputInt(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aTrayState"></param>
        public void SyncTrayState(out String aTrayState)
        {
            SyncTrayStateLinnCoUkSdp1 sync = new SyncTrayStateLinnCoUkSdp1(this);
            BeginTrayState(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aTrayState = sync.TrayState();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndTrayState().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginTrayState(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionTrayState, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentString((ParameterString)iActionTrayState.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aTrayState"></param>
        public void EndTrayState(IntPtr aAsyncHandle, out String aTrayState)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aTrayState = Invocation.OutputString(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aDiscState"></param>
        public void SyncDiscState(out String aDiscState)
        {
            SyncDiscStateLinnCoUkSdp1 sync = new SyncDiscStateLinnCoUkSdp1(this);
            BeginDiscState(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aDiscState = sync.DiscState();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndDiscState().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginDiscState(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionDiscState, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentString((ParameterString)iActionDiscState.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aDiscState"></param>
        public void EndDiscState(IntPtr aAsyncHandle, out String aDiscState)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aDiscState = Invocation.OutputString(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aPlayState"></param>
        public void SyncPlayState(out String aPlayState)
        {
            SyncPlayStateLinnCoUkSdp1 sync = new SyncPlayStateLinnCoUkSdp1(this);
            BeginPlayState(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aPlayState = sync.PlayState();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndPlayState().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginPlayState(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionPlayState, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentString((ParameterString)iActionPlayState.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aPlayState"></param>
        public void EndPlayState(IntPtr aAsyncHandle, out String aPlayState)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aPlayState = Invocation.OutputString(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aSearchType"></param>
        public void SyncSearchType(out String aSearchType)
        {
            SyncSearchTypeLinnCoUkSdp1 sync = new SyncSearchTypeLinnCoUkSdp1(this);
            BeginSearchType(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aSearchType = sync.SearchType();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndSearchType().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginSearchType(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionSearchType, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentString((ParameterString)iActionSearchType.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aSearchType"></param>
        public void EndSearchType(IntPtr aAsyncHandle, out String aSearchType)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aSearchType = Invocation.OutputString(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aSearchSpeed"></param>
        public void SyncSearchSpeed(out int aSearchSpeed)
        {
            SyncSearchSpeedLinnCoUkSdp1 sync = new SyncSearchSpeedLinnCoUkSdp1(this);
            BeginSearchSpeed(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aSearchSpeed = sync.SearchSpeed();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndSearchSpeed().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginSearchSpeed(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionSearchSpeed, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentInt((ParameterInt)iActionSearchSpeed.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aSearchSpeed"></param>
        public void EndSearchSpeed(IntPtr aAsyncHandle, out int aSearchSpeed)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aSearchSpeed = Invocation.OutputInt(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aTrack"></param>
        public void SyncTrack(out int aTrack)
        {
            SyncTrackLinnCoUkSdp1 sync = new SyncTrackLinnCoUkSdp1(this);
            BeginTrack(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aTrack = sync.Track();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndTrack().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginTrack(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionTrack, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentInt((ParameterInt)iActionTrack.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aTrack"></param>
        public void EndTrack(IntPtr aAsyncHandle, out int aTrack)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aTrack = Invocation.OutputInt(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aTrackElapsedTime"></param>
        public void SyncTrackElapsedTime(out String aTrackElapsedTime)
        {
            SyncTrackElapsedTimeLinnCoUkSdp1 sync = new SyncTrackElapsedTimeLinnCoUkSdp1(this);
            BeginTrackElapsedTime(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aTrackElapsedTime = sync.TrackElapsedTime();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndTrackElapsedTime().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginTrackElapsedTime(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionTrackElapsedTime, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentString((ParameterString)iActionTrackElapsedTime.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aTrackElapsedTime"></param>
        public void EndTrackElapsedTime(IntPtr aAsyncHandle, out String aTrackElapsedTime)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aTrackElapsedTime = Invocation.OutputString(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aTrackRemainingTime"></param>
        public void SyncTrackRemainingTime(out String aTrackRemainingTime)
        {
            SyncTrackRemainingTimeLinnCoUkSdp1 sync = new SyncTrackRemainingTimeLinnCoUkSdp1(this);
            BeginTrackRemainingTime(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aTrackRemainingTime = sync.TrackRemainingTime();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndTrackRemainingTime().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginTrackRemainingTime(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionTrackRemainingTime, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentString((ParameterString)iActionTrackRemainingTime.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aTrackRemainingTime"></param>
        public void EndTrackRemainingTime(IntPtr aAsyncHandle, out String aTrackRemainingTime)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aTrackRemainingTime = Invocation.OutputString(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aDiscElapsedTime"></param>
        public void SyncDiscElapsedTime(out String aDiscElapsedTime)
        {
            SyncDiscElapsedTimeLinnCoUkSdp1 sync = new SyncDiscElapsedTimeLinnCoUkSdp1(this);
            BeginDiscElapsedTime(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aDiscElapsedTime = sync.DiscElapsedTime();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndDiscElapsedTime().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginDiscElapsedTime(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionDiscElapsedTime, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentString((ParameterString)iActionDiscElapsedTime.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aDiscElapsedTime"></param>
        public void EndDiscElapsedTime(IntPtr aAsyncHandle, out String aDiscElapsedTime)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aDiscElapsedTime = Invocation.OutputString(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aDiscRemainingTime"></param>
        public void SyncDiscRemainingTime(out String aDiscRemainingTime)
        {
            SyncDiscRemainingTimeLinnCoUkSdp1 sync = new SyncDiscRemainingTimeLinnCoUkSdp1(this);
            BeginDiscRemainingTime(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aDiscRemainingTime = sync.DiscRemainingTime();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndDiscRemainingTime().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginDiscRemainingTime(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionDiscRemainingTime, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentString((ParameterString)iActionDiscRemainingTime.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aDiscRemainingTime"></param>
        public void EndDiscRemainingTime(IntPtr aAsyncHandle, out String aDiscRemainingTime)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aDiscRemainingTime = Invocation.OutputString(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aRepeatMode"></param>
        public void SyncRepeatMode(out String aRepeatMode)
        {
            SyncRepeatModeLinnCoUkSdp1 sync = new SyncRepeatModeLinnCoUkSdp1(this);
            BeginRepeatMode(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aRepeatMode = sync.RepeatMode();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndRepeatMode().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginRepeatMode(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionRepeatMode, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentString((ParameterString)iActionRepeatMode.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aRepeatMode"></param>
        public void EndRepeatMode(IntPtr aAsyncHandle, out String aRepeatMode)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aRepeatMode = Invocation.OutputString(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aIntroMode"></param>
        public void SyncIntroMode(out bool aIntroMode)
        {
            SyncIntroModeLinnCoUkSdp1 sync = new SyncIntroModeLinnCoUkSdp1(this);
            BeginIntroMode(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aIntroMode = sync.IntroMode();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndIntroMode().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginIntroMode(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionIntroMode, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentBool((ParameterBool)iActionIntroMode.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aIntroMode"></param>
        public void EndIntroMode(IntPtr aAsyncHandle, out bool aIntroMode)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aIntroMode = Invocation.OutputBool(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aProgramMode"></param>
        public void SyncProgramMode(out String aProgramMode)
        {
            SyncProgramModeLinnCoUkSdp1 sync = new SyncProgramModeLinnCoUkSdp1(this);
            BeginProgramMode(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aProgramMode = sync.ProgramMode();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndProgramMode().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginProgramMode(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionProgramMode, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentString((ParameterString)iActionProgramMode.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aProgramMode"></param>
        public void EndProgramMode(IntPtr aAsyncHandle, out String aProgramMode)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aProgramMode = Invocation.OutputString(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aDomain"></param>
        public void SyncDomain(out String aDomain)
        {
            SyncDomainLinnCoUkSdp1 sync = new SyncDomainLinnCoUkSdp1(this);
            BeginDomain(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aDomain = sync.Domain();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndDomain().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginDomain(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionDomain, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentString((ParameterString)iActionDomain.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aDomain"></param>
        public void EndDomain(IntPtr aAsyncHandle, out String aDomain)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aDomain = Invocation.OutputString(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aAngle"></param>
        public void SyncAngle(out int aAngle)
        {
            SyncAngleLinnCoUkSdp1 sync = new SyncAngleLinnCoUkSdp1(this);
            BeginAngle(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aAngle = sync.Angle();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndAngle().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginAngle(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionAngle, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentInt((ParameterInt)iActionAngle.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aAngle"></param>
        public void EndAngle(IntPtr aAsyncHandle, out int aAngle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aAngle = Invocation.OutputInt(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aTotalAngles"></param>
        public void SyncTotalAngles(out int aTotalAngles)
        {
            SyncTotalAnglesLinnCoUkSdp1 sync = new SyncTotalAnglesLinnCoUkSdp1(this);
            BeginTotalAngles(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aTotalAngles = sync.TotalAngles();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndTotalAngles().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginTotalAngles(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionTotalAngles, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentInt((ParameterInt)iActionTotalAngles.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aTotalAngles"></param>
        public void EndTotalAngles(IntPtr aAsyncHandle, out int aTotalAngles)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aTotalAngles = Invocation.OutputInt(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aSubtitle"></param>
        public void SyncSubtitle(out int aSubtitle)
        {
            SyncSubtitleLinnCoUkSdp1 sync = new SyncSubtitleLinnCoUkSdp1(this);
            BeginSubtitle(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aSubtitle = sync.Subtitle();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndSubtitle().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginSubtitle(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionSubtitle, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentInt((ParameterInt)iActionSubtitle.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aSubtitle"></param>
        public void EndSubtitle(IntPtr aAsyncHandle, out int aSubtitle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aSubtitle = Invocation.OutputInt(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aAudioTrack"></param>
        public void SyncAudioTrack(out int aAudioTrack)
        {
            SyncAudioTrackLinnCoUkSdp1 sync = new SyncAudioTrackLinnCoUkSdp1(this);
            BeginAudioTrack(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aAudioTrack = sync.AudioTrack();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndAudioTrack().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginAudioTrack(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionAudioTrack, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentInt((ParameterInt)iActionAudioTrack.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aAudioTrack"></param>
        public void EndAudioTrack(IntPtr aAsyncHandle, out int aAudioTrack)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aAudioTrack = Invocation.OutputInt(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aZoomLevel"></param>
        public void SyncZoomLevel(out String aZoomLevel)
        {
            SyncZoomLevelLinnCoUkSdp1 sync = new SyncZoomLevelLinnCoUkSdp1(this);
            BeginZoomLevel(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aZoomLevel = sync.ZoomLevel();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndZoomLevel().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginZoomLevel(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionZoomLevel, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentString((ParameterString)iActionZoomLevel.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aZoomLevel"></param>
        public void EndZoomLevel(IntPtr aAsyncHandle, out String aZoomLevel)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aZoomLevel = Invocation.OutputString(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aSetupMode"></param>
        public void SyncSetupMode(out bool aSetupMode)
        {
            SyncSetupModeLinnCoUkSdp1 sync = new SyncSetupModeLinnCoUkSdp1(this);
            BeginSetupMode(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aSetupMode = sync.SetupMode();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndSetupMode().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginSetupMode(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionSetupMode, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentBool((ParameterBool)iActionSetupMode.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aSetupMode"></param>
        public void EndSetupMode(IntPtr aAsyncHandle, out bool aSetupMode)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aSetupMode = Invocation.OutputBool(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aSacdState"></param>
        public void SyncSacdState(out String aSacdState)
        {
            SyncSacdStateLinnCoUkSdp1 sync = new SyncSacdStateLinnCoUkSdp1(this);
            BeginSacdState(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aSacdState = sync.SacdState();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndSacdState().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginSacdState(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionSacdState, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentString((ParameterString)iActionSacdState.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aSacdState"></param>
        public void EndSacdState(IntPtr aAsyncHandle, out String aSacdState)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aSacdState = Invocation.OutputString(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aSlideshow"></param>
        public void SyncSlideshow(out String aSlideshow)
        {
            SyncSlideshowLinnCoUkSdp1 sync = new SyncSlideshowLinnCoUkSdp1(this);
            BeginSlideshow(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aSlideshow = sync.Slideshow();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndSlideshow().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginSlideshow(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionSlideshow, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentString((ParameterString)iActionSlideshow.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aSlideshow"></param>
        public void EndSlideshow(IntPtr aAsyncHandle, out String aSlideshow)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aSlideshow = Invocation.OutputString(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aError"></param>
        public void SyncError(out String aError)
        {
            SyncErrorLinnCoUkSdp1 sync = new SyncErrorLinnCoUkSdp1(this);
            BeginError(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aError = sync.Error();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndError().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginError(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionError, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentString((ParameterString)iActionError.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aError"></param>
        public void EndError(IntPtr aAsyncHandle, out String aError)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aError = Invocation.OutputString(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aOrientation"></param>
        public void SyncOrientation(out String aOrientation)
        {
            SyncOrientationLinnCoUkSdp1 sync = new SyncOrientationLinnCoUkSdp1(this);
            BeginOrientation(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aOrientation = sync.Orientation();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndOrientation().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginOrientation(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionOrientation, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentString((ParameterString)iActionOrientation.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aOrientation"></param>
        public void EndOrientation(IntPtr aAsyncHandle, out String aOrientation)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aOrientation = Invocation.OutputString(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aDiscLength"></param>
        public void SyncDiscLength(out String aDiscLength)
        {
            SyncDiscLengthLinnCoUkSdp1 sync = new SyncDiscLengthLinnCoUkSdp1(this);
            BeginDiscLength(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aDiscLength = sync.DiscLength();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndDiscLength().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginDiscLength(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionDiscLength, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentString((ParameterString)iActionDiscLength.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aDiscLength"></param>
        public void EndDiscLength(IntPtr aAsyncHandle, out String aDiscLength)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aDiscLength = Invocation.OutputString(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aTrackLength"></param>
        public void SyncTrackLength(out String aTrackLength)
        {
            SyncTrackLengthLinnCoUkSdp1 sync = new SyncTrackLengthLinnCoUkSdp1(this);
            BeginTrackLength(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aTrackLength = sync.TrackLength();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndTrackLength().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginTrackLength(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionTrackLength, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentString((ParameterString)iActionTrackLength.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aTrackLength"></param>
        public void EndTrackLength(IntPtr aAsyncHandle, out String aTrackLength)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aTrackLength = Invocation.OutputString(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aTotalTracks"></param>
        public void SyncTotalTracks(out int aTotalTracks)
        {
            SyncTotalTracksLinnCoUkSdp1 sync = new SyncTotalTracksLinnCoUkSdp1(this);
            BeginTotalTracks(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aTotalTracks = sync.TotalTracks();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndTotalTracks().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginTotalTracks(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionTotalTracks, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentInt((ParameterInt)iActionTotalTracks.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aTotalTracks"></param>
        public void EndTotalTracks(IntPtr aAsyncHandle, out int aTotalTracks)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aTotalTracks = Invocation.OutputInt(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aTotalTitles"></param>
        public void SyncTotalTitles(out int aTotalTitles)
        {
            SyncTotalTitlesLinnCoUkSdp1 sync = new SyncTotalTitlesLinnCoUkSdp1(this);
            BeginTotalTitles(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aTotalTitles = sync.TotalTitles();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndTotalTitles().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginTotalTitles(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionTotalTitles, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentInt((ParameterInt)iActionTotalTitles.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aTotalTitles"></param>
        public void EndTotalTitles(IntPtr aAsyncHandle, out int aTotalTitles)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aTotalTitles = Invocation.OutputInt(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aGenre"></param>
        public void SyncGenre(out String aGenre)
        {
            SyncGenreLinnCoUkSdp1 sync = new SyncGenreLinnCoUkSdp1(this);
            BeginGenre(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aGenre = sync.Genre();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndGenre().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginGenre(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionGenre, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentString((ParameterString)iActionGenre.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aGenre"></param>
        public void EndGenre(IntPtr aAsyncHandle, out String aGenre)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aGenre = Invocation.OutputString(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aEncoding"></param>
        public void SyncEncoding(out uint aEncoding)
        {
            SyncEncodingLinnCoUkSdp1 sync = new SyncEncodingLinnCoUkSdp1(this);
            BeginEncoding(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aEncoding = sync.Encoding();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndEncoding().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginEncoding(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionEncoding, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentUint((ParameterUint)iActionEncoding.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aEncoding"></param>
        public void EndEncoding(IntPtr aAsyncHandle, out uint aEncoding)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aEncoding = Invocation.OutputUint(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aFileSize"></param>
        public void SyncFileSize(out uint aFileSize)
        {
            SyncFileSizeLinnCoUkSdp1 sync = new SyncFileSizeLinnCoUkSdp1(this);
            BeginFileSize(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aFileSize = sync.FileSize();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndFileSize().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginFileSize(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionFileSize, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentUint((ParameterUint)iActionFileSize.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aFileSize"></param>
        public void EndFileSize(IntPtr aAsyncHandle, out uint aFileSize)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aFileSize = Invocation.OutputUint(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aDiscId"></param>
        public void SyncDiscId(out uint aDiscId)
        {
            SyncDiscIdLinnCoUkSdp1 sync = new SyncDiscIdLinnCoUkSdp1(this);
            BeginDiscId(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aDiscId = sync.DiscId();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndDiscId().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginDiscId(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionDiscId, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentUint((ParameterUint)iActionDiscId.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aDiscId"></param>
        public void EndDiscId(IntPtr aAsyncHandle, out uint aDiscId)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aDiscId = Invocation.OutputUint(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aYear"></param>
        public void SyncYear(out String aYear)
        {
            SyncYearLinnCoUkSdp1 sync = new SyncYearLinnCoUkSdp1(this);
            BeginYear(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aYear = sync.Year();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndYear().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginYear(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionYear, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentString((ParameterString)iActionYear.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aYear"></param>
        public void EndYear(IntPtr aAsyncHandle, out String aYear)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aYear = Invocation.OutputString(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aTrackName"></param>
        public void SyncTrackName(out String aTrackName)
        {
            SyncTrackNameLinnCoUkSdp1 sync = new SyncTrackNameLinnCoUkSdp1(this);
            BeginTrackName(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aTrackName = sync.TrackName();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndTrackName().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginTrackName(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionTrackName, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentString((ParameterString)iActionTrackName.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aTrackName"></param>
        public void EndTrackName(IntPtr aAsyncHandle, out String aTrackName)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aTrackName = Invocation.OutputString(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aArtistName"></param>
        public void SyncArtistName(out String aArtistName)
        {
            SyncArtistNameLinnCoUkSdp1 sync = new SyncArtistNameLinnCoUkSdp1(this);
            BeginArtistName(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aArtistName = sync.ArtistName();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndArtistName().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginArtistName(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionArtistName, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentString((ParameterString)iActionArtistName.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aArtistName"></param>
        public void EndArtistName(IntPtr aAsyncHandle, out String aArtistName)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aArtistName = Invocation.OutputString(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aAlbumName"></param>
        public void SyncAlbumName(out String aAlbumName)
        {
            SyncAlbumNameLinnCoUkSdp1 sync = new SyncAlbumNameLinnCoUkSdp1(this);
            BeginAlbumName(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aAlbumName = sync.AlbumName();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndAlbumName().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginAlbumName(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionAlbumName, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentString((ParameterString)iActionAlbumName.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aAlbumName"></param>
        public void EndAlbumName(IntPtr aAsyncHandle, out String aAlbumName)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aAlbumName = Invocation.OutputString(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aComment"></param>
        public void SyncComment(out String aComment)
        {
            SyncCommentLinnCoUkSdp1 sync = new SyncCommentLinnCoUkSdp1(this);
            BeginComment(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aComment = sync.Comment();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndComment().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginComment(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionComment, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentString((ParameterString)iActionComment.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aComment"></param>
        public void EndComment(IntPtr aAsyncHandle, out String aComment)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aComment = Invocation.OutputString(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aIndex"></param>
        /// <param name="aFileName"></param>
        public void SyncFileName(int aIndex, out String aFileName)
        {
            SyncFileNameLinnCoUkSdp1 sync = new SyncFileNameLinnCoUkSdp1(this);
            BeginFileName(aIndex, sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aFileName = sync.FileName();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndFileName().</remarks>
        /// <param name="aIndex"></param>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginFileName(int aIndex, CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionFileName, aCallback);
            int inIndex = 0;
            invocation.AddInput(new ArgumentInt((ParameterInt)iActionFileName.InputParameter(inIndex++), aIndex));
            int outIndex = 0;
            invocation.AddOutput(new ArgumentString((ParameterString)iActionFileName.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aFileName"></param>
        public void EndFileName(IntPtr aAsyncHandle, out String aFileName)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aFileName = Invocation.OutputString(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aSystemCapabilities"></param>
        public void SyncSystemCapabilities(out byte[] aSystemCapabilities)
        {
            SyncSystemCapabilitiesLinnCoUkSdp1 sync = new SyncSystemCapabilitiesLinnCoUkSdp1(this);
            BeginSystemCapabilities(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aSystemCapabilities = sync.SystemCapabilities();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndSystemCapabilities().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginSystemCapabilities(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionSystemCapabilities, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentBinary((ParameterBinary)iActionSystemCapabilities.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aSystemCapabilities"></param>
        public void EndSystemCapabilities(IntPtr aAsyncHandle, out byte[] aSystemCapabilities)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aSystemCapabilities = Invocation.OutputBinary(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aDiscCapabilities"></param>
        public void SyncDiscCapabilities(out byte[] aDiscCapabilities)
        {
            SyncDiscCapabilitiesLinnCoUkSdp1 sync = new SyncDiscCapabilitiesLinnCoUkSdp1(this);
            BeginDiscCapabilities(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aDiscCapabilities = sync.DiscCapabilities();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndDiscCapabilities().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginDiscCapabilities(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionDiscCapabilities, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentBinary((ParameterBinary)iActionDiscCapabilities.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aDiscCapabilities"></param>
        public void EndDiscCapabilities(IntPtr aAsyncHandle, out byte[] aDiscCapabilities)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aDiscCapabilities = Invocation.OutputBinary(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aZoomLevelInfo"></param>
        public void SyncZoomLevelInfo(out byte[] aZoomLevelInfo)
        {
            SyncZoomLevelInfoLinnCoUkSdp1 sync = new SyncZoomLevelInfoLinnCoUkSdp1(this);
            BeginZoomLevelInfo(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aZoomLevelInfo = sync.ZoomLevelInfo();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndZoomLevelInfo().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginZoomLevelInfo(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionZoomLevelInfo, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentBinary((ParameterBinary)iActionZoomLevelInfo.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aZoomLevelInfo"></param>
        public void EndZoomLevelInfo(IntPtr aAsyncHandle, out byte[] aZoomLevelInfo)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aZoomLevelInfo = Invocation.OutputBinary(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aSubtitleInfo"></param>
        public void SyncSubtitleInfo(out byte[] aSubtitleInfo)
        {
            SyncSubtitleInfoLinnCoUkSdp1 sync = new SyncSubtitleInfoLinnCoUkSdp1(this);
            BeginSubtitleInfo(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aSubtitleInfo = sync.SubtitleInfo();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndSubtitleInfo().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginSubtitleInfo(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionSubtitleInfo, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentBinary((ParameterBinary)iActionSubtitleInfo.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aSubtitleInfo"></param>
        public void EndSubtitleInfo(IntPtr aAsyncHandle, out byte[] aSubtitleInfo)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aSubtitleInfo = Invocation.OutputBinary(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aAudioTrackInfo"></param>
        public void SyncAudioTrackInfo(out byte[] aAudioTrackInfo)
        {
            SyncAudioTrackInfoLinnCoUkSdp1 sync = new SyncAudioTrackInfoLinnCoUkSdp1(this);
            BeginAudioTrackInfo(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aAudioTrackInfo = sync.AudioTrackInfo();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndAudioTrackInfo().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginAudioTrackInfo(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionAudioTrackInfo, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentBinary((ParameterBinary)iActionAudioTrackInfo.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aAudioTrackInfo"></param>
        public void EndAudioTrackInfo(IntPtr aAsyncHandle, out byte[] aAudioTrackInfo)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aAudioTrackInfo = Invocation.OutputBinary(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aTableOfContents"></param>
        public void SyncTableOfContents(out byte[] aTableOfContents)
        {
            SyncTableOfContentsLinnCoUkSdp1 sync = new SyncTableOfContentsLinnCoUkSdp1(this);
            BeginTableOfContents(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aTableOfContents = sync.TableOfContents();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndTableOfContents().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginTableOfContents(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionTableOfContents, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentBinary((ParameterBinary)iActionTableOfContents.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aTableOfContents"></param>
        public void EndTableOfContents(IntPtr aAsyncHandle, out byte[] aTableOfContents)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aTableOfContents = Invocation.OutputBinary(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aDirectoryStructure"></param>
        public void SyncDirectoryStructure(out byte[] aDirectoryStructure)
        {
            SyncDirectoryStructureLinnCoUkSdp1 sync = new SyncDirectoryStructureLinnCoUkSdp1(this);
            BeginDirectoryStructure(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aDirectoryStructure = sync.DirectoryStructure();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndDirectoryStructure().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginDirectoryStructure(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionDirectoryStructure, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentBinary((ParameterBinary)iActionDirectoryStructure.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aDirectoryStructure"></param>
        public void EndDirectoryStructure(IntPtr aAsyncHandle, out byte[] aDirectoryStructure)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aDirectoryStructure = Invocation.OutputBinary(aAsyncHandle, index++);
        }

        /// <summary>
        /// Set a delegate to be run when the DiscType state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aDiscTypeChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyDiscTypeChanged(System.Action aDiscTypeChanged)
        {
            lock (iPropertyLock)
            {
                iDiscTypeChanged = aDiscTypeChanged;
            }
        }

        private void DiscTypePropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iDiscTypeChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the Title state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aTitleChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyTitleChanged(System.Action aTitleChanged)
        {
            lock (iPropertyLock)
            {
                iTitleChanged = aTitleChanged;
            }
        }

        private void TitlePropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iTitleChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the TrayState state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aTrayStateChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyTrayStateChanged(System.Action aTrayStateChanged)
        {
            lock (iPropertyLock)
            {
                iTrayStateChanged = aTrayStateChanged;
            }
        }

        private void TrayStatePropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iTrayStateChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the DiscState state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aDiscStateChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyDiscStateChanged(System.Action aDiscStateChanged)
        {
            lock (iPropertyLock)
            {
                iDiscStateChanged = aDiscStateChanged;
            }
        }

        private void DiscStatePropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iDiscStateChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the PlayState state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aPlayStateChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyPlayStateChanged(System.Action aPlayStateChanged)
        {
            lock (iPropertyLock)
            {
                iPlayStateChanged = aPlayStateChanged;
            }
        }

        private void PlayStatePropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iPlayStateChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the SearchType state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aSearchTypeChanged">The delegate to run when the state variable changes</param>
        public void SetPropertySearchTypeChanged(System.Action aSearchTypeChanged)
        {
            lock (iPropertyLock)
            {
                iSearchTypeChanged = aSearchTypeChanged;
            }
        }

        private void SearchTypePropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iSearchTypeChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the SearchSpeed state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aSearchSpeedChanged">The delegate to run when the state variable changes</param>
        public void SetPropertySearchSpeedChanged(System.Action aSearchSpeedChanged)
        {
            lock (iPropertyLock)
            {
                iSearchSpeedChanged = aSearchSpeedChanged;
            }
        }

        private void SearchSpeedPropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iSearchSpeedChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the Track state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aTrackChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyTrackChanged(System.Action aTrackChanged)
        {
            lock (iPropertyLock)
            {
                iTrackChanged = aTrackChanged;
            }
        }

        private void TrackPropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iTrackChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the RepeatMode state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aRepeatModeChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyRepeatModeChanged(System.Action aRepeatModeChanged)
        {
            lock (iPropertyLock)
            {
                iRepeatModeChanged = aRepeatModeChanged;
            }
        }

        private void RepeatModePropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iRepeatModeChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the IntroMode state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aIntroModeChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyIntroModeChanged(System.Action aIntroModeChanged)
        {
            lock (iPropertyLock)
            {
                iIntroModeChanged = aIntroModeChanged;
            }
        }

        private void IntroModePropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iIntroModeChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the ProgramMode state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aProgramModeChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyProgramModeChanged(System.Action aProgramModeChanged)
        {
            lock (iPropertyLock)
            {
                iProgramModeChanged = aProgramModeChanged;
            }
        }

        private void ProgramModePropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iProgramModeChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the Domain state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aDomainChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyDomainChanged(System.Action aDomainChanged)
        {
            lock (iPropertyLock)
            {
                iDomainChanged = aDomainChanged;
            }
        }

        private void DomainPropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iDomainChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the Angle state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aAngleChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyAngleChanged(System.Action aAngleChanged)
        {
            lock (iPropertyLock)
            {
                iAngleChanged = aAngleChanged;
            }
        }

        private void AnglePropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iAngleChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the TotalAngles state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aTotalAnglesChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyTotalAnglesChanged(System.Action aTotalAnglesChanged)
        {
            lock (iPropertyLock)
            {
                iTotalAnglesChanged = aTotalAnglesChanged;
            }
        }

        private void TotalAnglesPropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iTotalAnglesChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the Subtitle state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aSubtitleChanged">The delegate to run when the state variable changes</param>
        public void SetPropertySubtitleChanged(System.Action aSubtitleChanged)
        {
            lock (iPropertyLock)
            {
                iSubtitleChanged = aSubtitleChanged;
            }
        }

        private void SubtitlePropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iSubtitleChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the AudioTrack state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aAudioTrackChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyAudioTrackChanged(System.Action aAudioTrackChanged)
        {
            lock (iPropertyLock)
            {
                iAudioTrackChanged = aAudioTrackChanged;
            }
        }

        private void AudioTrackPropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iAudioTrackChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the ZoomLevel state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aZoomLevelChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyZoomLevelChanged(System.Action aZoomLevelChanged)
        {
            lock (iPropertyLock)
            {
                iZoomLevelChanged = aZoomLevelChanged;
            }
        }

        private void ZoomLevelPropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iZoomLevelChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the SetupMode state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aSetupModeChanged">The delegate to run when the state variable changes</param>
        public void SetPropertySetupModeChanged(System.Action aSetupModeChanged)
        {
            lock (iPropertyLock)
            {
                iSetupModeChanged = aSetupModeChanged;
            }
        }

        private void SetupModePropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iSetupModeChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the SacdState state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aSacdStateChanged">The delegate to run when the state variable changes</param>
        public void SetPropertySacdStateChanged(System.Action aSacdStateChanged)
        {
            lock (iPropertyLock)
            {
                iSacdStateChanged = aSacdStateChanged;
            }
        }

        private void SacdStatePropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iSacdStateChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the Slideshow state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aSlideshowChanged">The delegate to run when the state variable changes</param>
        public void SetPropertySlideshowChanged(System.Action aSlideshowChanged)
        {
            lock (iPropertyLock)
            {
                iSlideshowChanged = aSlideshowChanged;
            }
        }

        private void SlideshowPropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iSlideshowChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the Error state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aErrorChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyErrorChanged(System.Action aErrorChanged)
        {
            lock (iPropertyLock)
            {
                iErrorChanged = aErrorChanged;
            }
        }

        private void ErrorPropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iErrorChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the Orientation state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aOrientationChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyOrientationChanged(System.Action aOrientationChanged)
        {
            lock (iPropertyLock)
            {
                iOrientationChanged = aOrientationChanged;
            }
        }

        private void OrientationPropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iOrientationChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the TotalTracks state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aTotalTracksChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyTotalTracksChanged(System.Action aTotalTracksChanged)
        {
            lock (iPropertyLock)
            {
                iTotalTracksChanged = aTotalTracksChanged;
            }
        }

        private void TotalTracksPropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iTotalTracksChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the TotalTitles state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aTotalTitlesChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyTotalTitlesChanged(System.Action aTotalTitlesChanged)
        {
            lock (iPropertyLock)
            {
                iTotalTitlesChanged = aTotalTitlesChanged;
            }
        }

        private void TotalTitlesPropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iTotalTitlesChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the Encoding state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aEncodingChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyEncodingChanged(System.Action aEncodingChanged)
        {
            lock (iPropertyLock)
            {
                iEncodingChanged = aEncodingChanged;
            }
        }

        private void EncodingPropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iEncodingChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the FileSize state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aFileSizeChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyFileSizeChanged(System.Action aFileSizeChanged)
        {
            lock (iPropertyLock)
            {
                iFileSizeChanged = aFileSizeChanged;
            }
        }

        private void FileSizePropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iFileSizeChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the DiscId state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aDiscIdChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyDiscIdChanged(System.Action aDiscIdChanged)
        {
            lock (iPropertyLock)
            {
                iDiscIdChanged = aDiscIdChanged;
            }
        }

        private void DiscIdPropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iDiscIdChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the DiscLength state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aDiscLengthChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyDiscLengthChanged(System.Action aDiscLengthChanged)
        {
            lock (iPropertyLock)
            {
                iDiscLengthChanged = aDiscLengthChanged;
            }
        }

        private void DiscLengthPropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iDiscLengthChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the TrackLength state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aTrackLengthChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyTrackLengthChanged(System.Action aTrackLengthChanged)
        {
            lock (iPropertyLock)
            {
                iTrackLengthChanged = aTrackLengthChanged;
            }
        }

        private void TrackLengthPropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iTrackLengthChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the Genre state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aGenreChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyGenreChanged(System.Action aGenreChanged)
        {
            lock (iPropertyLock)
            {
                iGenreChanged = aGenreChanged;
            }
        }

        private void GenrePropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iGenreChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the Year state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aYearChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyYearChanged(System.Action aYearChanged)
        {
            lock (iPropertyLock)
            {
                iYearChanged = aYearChanged;
            }
        }

        private void YearPropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iYearChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the TrackName state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aTrackNameChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyTrackNameChanged(System.Action aTrackNameChanged)
        {
            lock (iPropertyLock)
            {
                iTrackNameChanged = aTrackNameChanged;
            }
        }

        private void TrackNamePropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iTrackNameChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the ArtistName state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aArtistNameChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyArtistNameChanged(System.Action aArtistNameChanged)
        {
            lock (iPropertyLock)
            {
                iArtistNameChanged = aArtistNameChanged;
            }
        }

        private void ArtistNamePropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iArtistNameChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the AlbumName state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aAlbumNameChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyAlbumNameChanged(System.Action aAlbumNameChanged)
        {
            lock (iPropertyLock)
            {
                iAlbumNameChanged = aAlbumNameChanged;
            }
        }

        private void AlbumNamePropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iAlbumNameChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the Comment state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aCommentChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyCommentChanged(System.Action aCommentChanged)
        {
            lock (iPropertyLock)
            {
                iCommentChanged = aCommentChanged;
            }
        }

        private void CommentPropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iCommentChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the FileName state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aFileNameChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyFileNameChanged(System.Action aFileNameChanged)
        {
            lock (iPropertyLock)
            {
                iFileNameChanged = aFileNameChanged;
            }
        }

        private void FileNamePropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iFileNameChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the SystemCapabilities state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aSystemCapabilitiesChanged">The delegate to run when the state variable changes</param>
        public void SetPropertySystemCapabilitiesChanged(System.Action aSystemCapabilitiesChanged)
        {
            lock (iPropertyLock)
            {
                iSystemCapabilitiesChanged = aSystemCapabilitiesChanged;
            }
        }

        private void SystemCapabilitiesPropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iSystemCapabilitiesChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the DiscCapabilities state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aDiscCapabilitiesChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyDiscCapabilitiesChanged(System.Action aDiscCapabilitiesChanged)
        {
            lock (iPropertyLock)
            {
                iDiscCapabilitiesChanged = aDiscCapabilitiesChanged;
            }
        }

        private void DiscCapabilitiesPropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iDiscCapabilitiesChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the ZoomLevelInfo state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aZoomLevelInfoChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyZoomLevelInfoChanged(System.Action aZoomLevelInfoChanged)
        {
            lock (iPropertyLock)
            {
                iZoomLevelInfoChanged = aZoomLevelInfoChanged;
            }
        }

        private void ZoomLevelInfoPropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iZoomLevelInfoChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the SubtitleInfo state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aSubtitleInfoChanged">The delegate to run when the state variable changes</param>
        public void SetPropertySubtitleInfoChanged(System.Action aSubtitleInfoChanged)
        {
            lock (iPropertyLock)
            {
                iSubtitleInfoChanged = aSubtitleInfoChanged;
            }
        }

        private void SubtitleInfoPropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iSubtitleInfoChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the AudioTrackInfo state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aAudioTrackInfoChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyAudioTrackInfoChanged(System.Action aAudioTrackInfoChanged)
        {
            lock (iPropertyLock)
            {
                iAudioTrackInfoChanged = aAudioTrackInfoChanged;
            }
        }

        private void AudioTrackInfoPropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iAudioTrackInfoChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the TableOfContents state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aTableOfContentsChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyTableOfContentsChanged(System.Action aTableOfContentsChanged)
        {
            lock (iPropertyLock)
            {
                iTableOfContentsChanged = aTableOfContentsChanged;
            }
        }

        private void TableOfContentsPropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iTableOfContentsChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the DirectoryStructure state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkSdp1 instance will not overlap.</remarks>
        /// <param name="aDirectoryStructureChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyDirectoryStructureChanged(System.Action aDirectoryStructureChanged)
        {
            lock (iPropertyLock)
            {
                iDirectoryStructureChanged = aDirectoryStructureChanged;
            }
        }

        private void DirectoryStructurePropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iDirectoryStructureChanged);
            }
        }

        /// <summary>
        /// Query the value of the DiscType property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the DiscType property</returns>
        public String PropertyDiscType()
        {
            PropertyReadLock();
            String val = iDiscType.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the Title property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the Title property</returns>
        public int PropertyTitle()
        {
            PropertyReadLock();
            int val = iTitle.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the TrayState property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the TrayState property</returns>
        public String PropertyTrayState()
        {
            PropertyReadLock();
            String val = iTrayState.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the DiscState property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the DiscState property</returns>
        public String PropertyDiscState()
        {
            PropertyReadLock();
            String val = iDiscState.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the PlayState property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the PlayState property</returns>
        public String PropertyPlayState()
        {
            PropertyReadLock();
            String val = iPlayState.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the SearchType property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the SearchType property</returns>
        public String PropertySearchType()
        {
            PropertyReadLock();
            String val = iSearchType.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the SearchSpeed property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the SearchSpeed property</returns>
        public int PropertySearchSpeed()
        {
            PropertyReadLock();
            int val = iSearchSpeed.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the Track property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the Track property</returns>
        public int PropertyTrack()
        {
            PropertyReadLock();
            int val = iTrack.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the RepeatMode property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the RepeatMode property</returns>
        public String PropertyRepeatMode()
        {
            PropertyReadLock();
            String val = iRepeatMode.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the IntroMode property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the IntroMode property</returns>
        public bool PropertyIntroMode()
        {
            PropertyReadLock();
            bool val = iIntroMode.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the ProgramMode property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the ProgramMode property</returns>
        public String PropertyProgramMode()
        {
            PropertyReadLock();
            String val = iProgramMode.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the Domain property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the Domain property</returns>
        public String PropertyDomain()
        {
            PropertyReadLock();
            String val = iDomain.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the Angle property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the Angle property</returns>
        public int PropertyAngle()
        {
            PropertyReadLock();
            int val = iAngle.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the TotalAngles property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the TotalAngles property</returns>
        public int PropertyTotalAngles()
        {
            PropertyReadLock();
            int val = iTotalAngles.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the Subtitle property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the Subtitle property</returns>
        public int PropertySubtitle()
        {
            PropertyReadLock();
            int val = iSubtitle.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the AudioTrack property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the AudioTrack property</returns>
        public int PropertyAudioTrack()
        {
            PropertyReadLock();
            int val = iAudioTrack.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the ZoomLevel property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the ZoomLevel property</returns>
        public String PropertyZoomLevel()
        {
            PropertyReadLock();
            String val = iZoomLevel.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the SetupMode property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the SetupMode property</returns>
        public bool PropertySetupMode()
        {
            PropertyReadLock();
            bool val = iSetupMode.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the SacdState property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the SacdState property</returns>
        public String PropertySacdState()
        {
            PropertyReadLock();
            String val = iSacdState.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the Slideshow property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the Slideshow property</returns>
        public String PropertySlideshow()
        {
            PropertyReadLock();
            String val = iSlideshow.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the Error property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the Error property</returns>
        public String PropertyError()
        {
            PropertyReadLock();
            String val = iError.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the Orientation property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the Orientation property</returns>
        public String PropertyOrientation()
        {
            PropertyReadLock();
            String val = iOrientation.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the TotalTracks property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the TotalTracks property</returns>
        public int PropertyTotalTracks()
        {
            PropertyReadLock();
            int val = iTotalTracks.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the TotalTitles property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the TotalTitles property</returns>
        public int PropertyTotalTitles()
        {
            PropertyReadLock();
            int val = iTotalTitles.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the Encoding property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the Encoding property</returns>
        public uint PropertyEncoding()
        {
            PropertyReadLock();
            uint val = iEncoding.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the FileSize property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the FileSize property</returns>
        public uint PropertyFileSize()
        {
            PropertyReadLock();
            uint val = iFileSize.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the DiscId property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the DiscId property</returns>
        public uint PropertyDiscId()
        {
            PropertyReadLock();
            uint val = iDiscId.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the DiscLength property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the DiscLength property</returns>
        public String PropertyDiscLength()
        {
            PropertyReadLock();
            String val = iDiscLength.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the TrackLength property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the TrackLength property</returns>
        public String PropertyTrackLength()
        {
            PropertyReadLock();
            String val = iTrackLength.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the Genre property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the Genre property</returns>
        public String PropertyGenre()
        {
            PropertyReadLock();
            String val = iGenre.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the Year property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the Year property</returns>
        public String PropertyYear()
        {
            PropertyReadLock();
            String val = iYear.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the TrackName property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the TrackName property</returns>
        public String PropertyTrackName()
        {
            PropertyReadLock();
            String val = iTrackName.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the ArtistName property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the ArtistName property</returns>
        public String PropertyArtistName()
        {
            PropertyReadLock();
            String val = iArtistName.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the AlbumName property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the AlbumName property</returns>
        public String PropertyAlbumName()
        {
            PropertyReadLock();
            String val = iAlbumName.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the Comment property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the Comment property</returns>
        public String PropertyComment()
        {
            PropertyReadLock();
            String val = iComment.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the FileName property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the FileName property</returns>
        public byte[] PropertyFileName()
        {
            PropertyReadLock();
            byte[] val = iFileName.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the SystemCapabilities property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the SystemCapabilities property</returns>
        public byte[] PropertySystemCapabilities()
        {
            PropertyReadLock();
            byte[] val = iSystemCapabilities.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the DiscCapabilities property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the DiscCapabilities property</returns>
        public byte[] PropertyDiscCapabilities()
        {
            PropertyReadLock();
            byte[] val = iDiscCapabilities.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the ZoomLevelInfo property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the ZoomLevelInfo property</returns>
        public byte[] PropertyZoomLevelInfo()
        {
            PropertyReadLock();
            byte[] val = iZoomLevelInfo.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the SubtitleInfo property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the SubtitleInfo property</returns>
        public byte[] PropertySubtitleInfo()
        {
            PropertyReadLock();
            byte[] val = iSubtitleInfo.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the AudioTrackInfo property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the AudioTrackInfo property</returns>
        public byte[] PropertyAudioTrackInfo()
        {
            PropertyReadLock();
            byte[] val = iAudioTrackInfo.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the TableOfContents property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the TableOfContents property</returns>
        public byte[] PropertyTableOfContents()
        {
            PropertyReadLock();
            byte[] val = iTableOfContents.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the DirectoryStructure property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the DirectoryStructure property</returns>
        public byte[] PropertyDirectoryStructure()
        {
            PropertyReadLock();
            byte[] val = iDirectoryStructure.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Must be called for each class instance.  Must be called before Core.Library.Close().
        /// </summary>
        public void Dispose()
        {
            lock (this)
            {
                if (iHandle == IntPtr.Zero)
                    return;
                DisposeProxy();
                iHandle = IntPtr.Zero;
            }
            iActionOpen.Dispose();
            iActionClose.Dispose();
            iActionPlay.Dispose();
            iActionStop.Dispose();
            iActionPause.Dispose();
            iActionResume.Dispose();
            iActionSearch.Dispose();
            iActionSetTrack.Dispose();
            iActionSetTime.Dispose();
            iActionSetProgramOff.Dispose();
            iActionSetProgramInclude.Dispose();
            iActionSetProgramExclude.Dispose();
            iActionSetProgramRandom.Dispose();
            iActionSetProgramShuffle.Dispose();
            iActionSetRepeatOff.Dispose();
            iActionSetRepeatAll.Dispose();
            iActionSetRepeatTrack.Dispose();
            iActionSetRepeatAb.Dispose();
            iActionSetIntroMode.Dispose();
            iActionSetNext.Dispose();
            iActionSetPrev.Dispose();
            iActionRootMenu.Dispose();
            iActionTitleMenu.Dispose();
            iActionSetSetupMode.Dispose();
            iActionSetAngle.Dispose();
            iActionSetAudioTrack.Dispose();
            iActionSetSubtitle.Dispose();
            iActionSetZoom.Dispose();
            iActionSetSacdLayer.Dispose();
            iActionNavigate.Dispose();
            iActionSetSlideshow.Dispose();
            iActionSetPassword.Dispose();
            iActionDiscType.Dispose();
            iActionTitle.Dispose();
            iActionTrayState.Dispose();
            iActionDiscState.Dispose();
            iActionPlayState.Dispose();
            iActionSearchType.Dispose();
            iActionSearchSpeed.Dispose();
            iActionTrack.Dispose();
            iActionTrackElapsedTime.Dispose();
            iActionTrackRemainingTime.Dispose();
            iActionDiscElapsedTime.Dispose();
            iActionDiscRemainingTime.Dispose();
            iActionRepeatMode.Dispose();
            iActionIntroMode.Dispose();
            iActionProgramMode.Dispose();
            iActionDomain.Dispose();
            iActionAngle.Dispose();
            iActionTotalAngles.Dispose();
            iActionSubtitle.Dispose();
            iActionAudioTrack.Dispose();
            iActionZoomLevel.Dispose();
            iActionSetupMode.Dispose();
            iActionSacdState.Dispose();
            iActionSlideshow.Dispose();
            iActionError.Dispose();
            iActionOrientation.Dispose();
            iActionDiscLength.Dispose();
            iActionTrackLength.Dispose();
            iActionTotalTracks.Dispose();
            iActionTotalTitles.Dispose();
            iActionGenre.Dispose();
            iActionEncoding.Dispose();
            iActionFileSize.Dispose();
            iActionDiscId.Dispose();
            iActionYear.Dispose();
            iActionTrackName.Dispose();
            iActionArtistName.Dispose();
            iActionAlbumName.Dispose();
            iActionComment.Dispose();
            iActionFileName.Dispose();
            iActionSystemCapabilities.Dispose();
            iActionDiscCapabilities.Dispose();
            iActionZoomLevelInfo.Dispose();
            iActionSubtitleInfo.Dispose();
            iActionAudioTrackInfo.Dispose();
            iActionTableOfContents.Dispose();
            iActionDirectoryStructure.Dispose();
            iDiscType.Dispose();
            iTitle.Dispose();
            iTrayState.Dispose();
            iDiscState.Dispose();
            iPlayState.Dispose();
            iSearchType.Dispose();
            iSearchSpeed.Dispose();
            iTrack.Dispose();
            iRepeatMode.Dispose();
            iIntroMode.Dispose();
            iProgramMode.Dispose();
            iDomain.Dispose();
            iAngle.Dispose();
            iTotalAngles.Dispose();
            iSubtitle.Dispose();
            iAudioTrack.Dispose();
            iZoomLevel.Dispose();
            iSetupMode.Dispose();
            iSacdState.Dispose();
            iSlideshow.Dispose();
            iError.Dispose();
            iOrientation.Dispose();
            iTotalTracks.Dispose();
            iTotalTitles.Dispose();
            iEncoding.Dispose();
            iFileSize.Dispose();
            iDiscId.Dispose();
            iDiscLength.Dispose();
            iTrackLength.Dispose();
            iGenre.Dispose();
            iYear.Dispose();
            iTrackName.Dispose();
            iArtistName.Dispose();
            iAlbumName.Dispose();
            iComment.Dispose();
            iFileName.Dispose();
            iSystemCapabilities.Dispose();
            iDiscCapabilities.Dispose();
            iZoomLevelInfo.Dispose();
            iSubtitleInfo.Dispose();
            iAudioTrackInfo.Dispose();
            iTableOfContents.Dispose();
            iDirectoryStructure.Dispose();
        }
    }
}

