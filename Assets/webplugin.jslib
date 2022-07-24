var WebPlugin = {
    IsMobile: function()
    {
        return UnityLoader.SystemInfo.mobile;
    }
};

mergeInto(LibraryManager.library, WebPlugin);
