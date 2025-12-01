using System.Text;
using Jellyfin.Plugin.TrailerPreview.Configuration;

namespace Jellyfin.Plugin.TrailerPreview.Helpers;

public static class ScriptBuilder
{
    public static string BuildClientScript(PluginConfiguration config)
    {
        var sb = new StringBuilder();

        sb.AppendLine("(function() {");
        sb.AppendLine("    'use strict';");
        sb.AppendLine();
        sb.AppendLine("    // Jellyfin Trailer Preview Plugin");
        sb.AppendLine($"    const CONFIG = {BuildConfigObject(config)};");
        sb.AppendLine();
        sb.AppendLine(GetCoreScript());
        sb.AppendLine("})();");

        return sb.ToString();
    }

    private static string BuildConfigObject(PluginConfiguration config)
    {
        return $@"{{
        enableTrailerPreview: {config.EnableTrailerPreview.ToString().ToLower()},
        enableDebugLogging: {config.EnableDebugLogging.ToString().ToLower()},
        hoverDelayMs: {config.HoverDelayMs},
        previewDurationMs: {config.PreviewDurationMs},
        fadeInDurationMs: {config.FadeInDurationMs},
        fadeOutDurationMs: {config.FadeOutDurationMs},
        positionMode: '{config.PositionMode}',
        offsetX: {config.OffsetX},
        offsetY: {config.OffsetY},
        sizeMode: '{config.SizeMode}',
        previewWidth: {config.PreviewWidth},
        previewHeight: {config.PreviewHeight},
        previewSizePercentage: {config.PreviewSizePercentage},
        previewOpacity: {config.PreviewOpacity},
        previewBorderRadius: {config.PreviewBorderRadius},
        enableBackgroundBlur: {config.EnableBackgroundBlur.ToString().ToLower()},
        enableBackgroundDim: {config.EnableBackgroundDim.ToString().ToLower()},
        backgroundDimOpacity: {config.BackgroundDimOpacity},
        enableAudio: {config.EnableAudio.ToString().ToLower()},
        volume: {config.Volume},
        muteByDefault: {config.MuteByDefault.ToString().ToLower()},
        videoQuality: '{config.VideoQuality}',
        autoPlayTrailer: {config.AutoPlayTrailer.ToString().ToLower()},
        loopTrailer: {config.LoopTrailer.ToString().ToLower()},
        preloadTrailers: {config.PreloadTrailers.ToString().ToLower()},
        showProgressBar: {config.ShowProgressBar.ToString().ToLower()},
        showCloseButton: {config.ShowCloseButton.ToString().ToLower()},
        showMuteButton: {config.ShowMuteButton.ToString().ToLower()},
        showTitle: {config.ShowTitle.ToString().ToLower()},
        enableOnTV: {config.EnableOnTV.ToString().ToLower()},
        enableOnMobile: {config.EnableOnMobile.ToString().ToLower()},
        enableOnDesktop: {config.EnableOnDesktop.ToString().ToLower()},
        enableOnWeb: {config.EnableOnWeb.ToString().ToLower()},
        onlyShowForItems: {config.OnlyShowForItems.ToString().ToLower()},
        itemTypes: {BuildArrayString(config.ItemTypes)},
        maxConcurrentPreviews: {config.MaxConcurrentPreviews},
        disableOnLowBandwidth: {config.DisableOnLowBandwidth.ToString().ToLower()}
    }}";
    }

    private static string BuildArrayString(string[] items)
    {
        var quoted = items.Select(i => $"'{i}'");
        return $"[{string.Join(", ", quoted)}]";
    }

    private static string GetCoreScript()
    {
        return @"
    // Logger utility
    const log = {
        debug: (...args) => CONFIG.enableDebugLogging && console.log('[TrailerPreview]', ...args),
        info: (...args) => console.info('[TrailerPreview]', ...args),
        warn: (...args) => console.warn('[TrailerPreview]', ...args),
        error: (...args) => console.error('[TrailerPreview]', ...args)
    };

    // Platform detection
    const platform = {
        isTV: navigator.userAgent.includes('TV') || navigator.userAgent.includes('SmartTV'),
        isMobile: /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent),
        isDesktop: !(/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent))
    };

    // Check if plugin should be enabled for current platform
    function isPlatformEnabled() {
        if (platform.isTV && !CONFIG.enableOnTV) return false;
        if (platform.isMobile && !CONFIG.enableOnMobile) return false;
        if (platform.isDesktop && !CONFIG.enableOnDesktop) return false;
        return true;
    }

    if (!isPlatformEnabled()) {
        log.info('Trailer Preview disabled for current platform');
        return;
    }

    // State management
    const state = {
        currentPreview: null,
        hoverTimer: null,
        durationTimer: null,
        activeCount: 0,
        trailerCache: new Map(),
        apiKey: null
    };

    // Get API key from localStorage or sessionStorage
    function getApiKey() {
        try {
            const authData = JSON.parse(localStorage.getItem('jellyfin_credentials') || '{}');
            return authData.AccessToken || sessionStorage.getItem('jellyfin-token') || null;
        } catch {
            return null;
        }
    }

    // Fetch trailer information
    async function fetchTrailerInfo(itemId) {
        if (state.trailerCache.has(itemId)) {
            return state.trailerCache.get(itemId);
        }

        try {
            const response = await fetch(`/TrailerPreview/TrailerInfo/${itemId}`);
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            const data = await response.json();

            if (data.isAvailable) {
                // Replace API key placeholder if needed
                if (data.trailerUrl && data.trailerUrl.includes('{API_KEY}')) {
                    const apiKey = getApiKey();
                    if (apiKey) {
                        data.trailerUrl = data.trailerUrl.replace('{API_KEY}', apiKey);
                    }
                }
                state.trailerCache.set(itemId, data);
            }

            return data;
        } catch (error) {
            log.error('Failed to fetch trailer info:', error);
            return null;
        }
    }

    // Create overlay container
    function createOverlay() {
        const overlay = document.createElement('div');
        overlay.id = 'trailer-preview-overlay';
        overlay.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            width: 100vw;
            height: 100vh;
            background: rgba(0, 0, 0, ${CONFIG.enableBackgroundDim ? CONFIG.backgroundDimOpacity : 0});
            ${CONFIG.enableBackgroundBlur ? 'backdrop-filter: blur(8px);' : ''}
            z-index: 9999;
            display: flex;
            align-items: center;
            justify-content: center;
            opacity: 0;
            transition: opacity ${CONFIG.fadeInDurationMs}ms ease-in-out;
            pointer-events: auto;
        `;

        // Click overlay to close
        overlay.addEventListener('click', (e) => {
            if (e.target === overlay) {
                closePreview();
            }
        });

        return overlay;
    }

    // Create video container
    function createVideoContainer(trailerInfo) {
        const container = document.createElement('div');
        container.className = 'trailer-preview-container';

        let width, height;
        if (CONFIG.sizeMode === 'Percentage') {
            width = `${CONFIG.previewSizePercentage}vw`;
            height = `${(CONFIG.previewSizePercentage * 9) / 16}vw`;
        } else {
            width = `${CONFIG.previewWidth}px`;
            height = `${CONFIG.previewHeight}px`;
        }

        const offsetX = CONFIG.positionMode === 'Custom' ? `${CONFIG.offsetX}px` : '0';
        const offsetY = CONFIG.positionMode === 'Custom' ? `${CONFIG.offsetY}px` : '0';

        container.style.cssText = `
            position: relative;
            width: ${width};
            height: ${height};
            max-width: 95vw;
            max-height: 95vh;
            background: #000;
            border-radius: ${CONFIG.previewBorderRadius}px;
            overflow: hidden;
            box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.5);
            opacity: ${CONFIG.previewOpacity};
            transform: translate(${offsetX}, ${offsetY}) scale(0.9);
            transition: transform ${CONFIG.fadeInDurationMs}ms cubic-bezier(0.34, 1.56, 0.64, 1);
        `;

        // Create video element
        let videoElement;
        if (trailerInfo.isLocal) {
            videoElement = document.createElement('video');
            videoElement.src = trailerInfo.trailerUrl;
            videoElement.autoplay = CONFIG.autoPlayTrailer;
            videoElement.muted = CONFIG.muteByDefault;
            videoElement.loop = CONFIG.loopTrailer;
            videoElement.controls = false;
            videoElement.volume = CONFIG.volume / 100;
        } else {
            videoElement = document.createElement('iframe');
            videoElement.src = trailerInfo.trailerUrl;
            videoElement.allow = 'autoplay; encrypted-media';
            videoElement.allowFullscreen = true;
        }

        videoElement.style.cssText = `
            width: 100%;
            height: 100%;
            border: none;
            display: block;
        `;

        container.appendChild(videoElement);

        // Add title if enabled
        if (CONFIG.showTitle && trailerInfo.title) {
            const title = document.createElement('div');
            title.textContent = trailerInfo.title;
            title.style.cssText = `
                position: absolute;
                top: 20px;
                left: 20px;
                right: 20px;
                color: white;
                font-size: 24px;
                font-weight: 600;
                text-shadow: 0 2px 4px rgba(0, 0, 0, 0.8);
                z-index: 10;
                pointer-events: none;
            `;
            container.appendChild(title);
        }

        // Add controls overlay
        const controls = createControls(videoElement, trailerInfo.isLocal);
        container.appendChild(controls);

        // Animate in
        requestAnimationFrame(() => {
            requestAnimationFrame(() => {
                container.style.transform = `translate(${offsetX}, ${offsetY}) scale(1)`;
            });
        });

        return { container, videoElement };
    }

    // Create controls
    function createControls(videoElement, isLocal) {
        const controls = document.createElement('div');
        controls.className = 'trailer-preview-controls';
        controls.style.cssText = `
            position: absolute;
            bottom: 0;
            left: 0;
            right: 0;
            padding: 20px;
            background: linear-gradient(to top, rgba(0,0,0,0.8), transparent);
            display: flex;
            gap: 12px;
            align-items: center;
            z-index: 10;
        `;

        // Mute button (only for local videos)
        if (CONFIG.showMuteButton && isLocal) {
            const muteBtn = createButton('🔇', () => {
                if (videoElement.muted) {
                    videoElement.muted = false;
                    muteBtn.textContent = '🔊';
                } else {
                    videoElement.muted = true;
                    muteBtn.textContent = '🔇';
                }
            });
            controls.appendChild(muteBtn);
        }

        // Progress bar
        if (CONFIG.showProgressBar && isLocal) {
            const progress = document.createElement('div');
            progress.style.cssText = `
                flex: 1;
                height: 4px;
                background: rgba(255,255,255,0.3);
                border-radius: 2px;
                overflow: hidden;
            `;

            const progressBar = document.createElement('div');
            progressBar.style.cssText = `
                height: 100%;
                width: 0%;
                background: #e50914;
                transition: width 0.3s;
            `;
            progress.appendChild(progressBar);

            videoElement.addEventListener('timeupdate', () => {
                const percent = (videoElement.currentTime / videoElement.duration) * 100;
                progressBar.style.width = percent + '%';
            });

            controls.appendChild(progress);
        }

        // Close button
        if (CONFIG.showCloseButton) {
            const closeBtn = createButton('✕', closePreview);
            controls.appendChild(closeBtn);
        }

        return controls;
    }

    // Create button helper
    function createButton(text, onClick) {
        const btn = document.createElement('button');
        btn.textContent = text;
        btn.style.cssText = `
            background: rgba(255,255,255,0.2);
            border: none;
            color: white;
            padding: 8px 16px;
            border-radius: 6px;
            cursor: pointer;
            font-size: 16px;
            transition: background 0.2s;
        `;
        btn.addEventListener('mouseenter', () => {
            btn.style.background = 'rgba(255,255,255,0.3)';
        });
        btn.addEventListener('mouseleave', () => {
            btn.style.background = 'rgba(255,255,255,0.2)';
        });
        btn.addEventListener('click', (e) => {
            e.stopPropagation();
            onClick();
        });
        return btn;
    }

    // Show preview
    async function showPreview(itemId) {
        if (state.activeCount >= CONFIG.maxConcurrentPreviews) {
            log.debug('Max concurrent previews reached');
            return;
        }

        log.debug('Showing preview for item:', itemId);

        const trailerInfo = await fetchTrailerInfo(itemId);
        if (!trailerInfo || !trailerInfo.isAvailable) {
            log.debug('No trailer available for item:', itemId);
            return;
        }

        // Create overlay and container
        const overlay = createOverlay();
        const { container, videoElement } = createVideoContainer(trailerInfo);

        overlay.appendChild(container);
        document.body.appendChild(overlay);

        // Fade in
        requestAnimationFrame(() => {
            overlay.style.opacity = '1';
        });

        state.currentPreview = { overlay, videoElement, itemId };
        state.activeCount++;

        // Auto-close after duration
        if (CONFIG.previewDurationMs > 0) {
            state.durationTimer = setTimeout(() => {
                closePreview();
            }, CONFIG.previewDurationMs);
        }
    }

    // Close preview
    function closePreview() {
        if (!state.currentPreview) return;

        const { overlay, videoElement } = state.currentPreview;

        // Fade out
        overlay.style.opacity = '0';

        setTimeout(() => {
            // Stop video
            if (videoElement.tagName === 'VIDEO') {
                videoElement.pause();
                videoElement.src = '';
            } else if (videoElement.tagName === 'IFRAME') {
                videoElement.src = 'about:blank';
            }

            // Remove from DOM
            overlay.remove();

            state.currentPreview = null;
            state.activeCount--;
        }, CONFIG.fadeOutDurationMs);

        // Clear timers
        if (state.durationTimer) {
            clearTimeout(state.durationTimer);
            state.durationTimer = null;
        }
    }

    // Setup hover listeners
    function setupHoverListeners() {
        const observer = new MutationObserver((mutations) => {
            mutations.forEach((mutation) => {
                mutation.addedNodes.forEach((node) => {
                    if (node.nodeType === 1) {
                        attachHoverListeners(node);
                    }
                });
            });
        });

        observer.observe(document.body, {
            childList: true,
            subtree: true
        });

        // Attach to existing elements
        attachHoverListeners(document.body);
    }

    // Attach hover listeners to element
    function attachHoverListeners(element) {
        // Find all card elements (adjust selectors based on Jellyfin's structure)
        const cards = element.querySelectorAll ? element.querySelectorAll(
            '.card:not([data-trailer-preview]), ' +
            '.itemTile:not([data-trailer-preview]), ' +
            '[data-type=\"Card\"]:not([data-trailer-preview]), ' +
            '.listItem:not([data-trailer-preview])'
        ) : [];

        cards.forEach(card => {
            card.setAttribute('data-trailer-preview', 'true');

            card.addEventListener('mouseenter', (e) => {
                // Clear any existing timer
                if (state.hoverTimer) {
                    clearTimeout(state.hoverTimer);
                }

                // Get item ID
                const itemId = card.getAttribute('data-id') ||
                              card.getAttribute('data-itemid') ||
                              card.querySelector('[data-id]')?.getAttribute('data-id');

                if (!itemId) {
                    log.debug('No item ID found for card');
                    return;
                }

                // Set timer for hover delay
                state.hoverTimer = setTimeout(() => {
                    showPreview(itemId);
                }, CONFIG.hoverDelayMs);
            });

            card.addEventListener('mouseleave', () => {
                // Clear hover timer
                if (state.hoverTimer) {
                    clearTimeout(state.hoverTimer);
                    state.hoverTimer = null;
                }
            });
        });
    }

    // Keyboard shortcuts
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape' && state.currentPreview) {
            closePreview();
        }
    });

    // Initialize
    function init() {
        log.info('Trailer Preview plugin initialized');
        log.debug('Configuration:', CONFIG);
        log.debug('Platform:', platform);

        // Wait for page to load
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', setupHoverListeners);
        } else {
            setupHoverListeners();
        }
    }

    init();
";
    }
}
