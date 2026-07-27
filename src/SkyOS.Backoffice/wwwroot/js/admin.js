(function () {
    'use strict';

    var STORAGE_KEY = 'skyos.backoffice.sidebarCollapsed';
    var body = document.body;
    var sidebar = document.getElementById('admin-sidebar');
    var toggle = document.getElementById('admin-sidebar-toggle');
    var overlay = document.getElementById('admin-sidebar-overlay');
    var mobileToggle = document.getElementById('admin-mobile-menu');

    function isCollapsed() {
        return body.classList.contains('admin-sidebar-collapsed');
    }

    function setCollapsed(collapsed) {
        body.classList.toggle('admin-sidebar-collapsed', collapsed);
        if (toggle) {
            toggle.setAttribute('aria-expanded', collapsed ? 'false' : 'true');
            var label = toggle.querySelector('.admin-sidebar-toggle-label');
            if (label) {
                label.textContent = collapsed
                    ? (toggle.getAttribute('data-label-expand') || 'Expand')
                    : (toggle.getAttribute('data-label-collapse') || 'Collapse');
            }
        }
        try {
            localStorage.setItem(STORAGE_KEY, collapsed ? '1' : '0');
        } catch (_) { /* ignore */ }
    }

    function closeMobile() {
        body.classList.remove('admin-sidebar-open');
    }

    function openMobile() {
        body.classList.add('admin-sidebar-open');
    }

    if (localStorage.getItem(STORAGE_KEY) === '1') {
        setCollapsed(true);
    }

    toggle?.addEventListener('click', function () {
        if (window.matchMedia('(max-width: 1024px)').matches) {
            closeMobile();
            return;
        }
        setCollapsed(!isCollapsed());
    });

    mobileToggle?.addEventListener('click', function () {
        if (body.classList.contains('admin-sidebar-open')) {
            closeMobile();
        } else {
            openMobile();
        }
    });

    overlay?.addEventListener('click', closeMobile);

    window.addEventListener('resize', function () {
        if (window.innerWidth > 1024) {
            closeMobile();
        }
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            closeMobile();
        }
    });

    // Shared modal state for image upload size choice
    var uploadModal = document.getElementById('admin-upload-modal');
    var uploadModalCtx = null;

    function formatUploadSize(bytes) {
        if (bytes < 1024) return bytes + ' B';
        if (bytes < 1048576) return (bytes / 1024).toFixed(1).replace(/\.0$/, '') + ' KB';
        return (bytes / 1048576).toFixed(1).replace(/\.0$/, '') + ' MB';
    }

    /** Estimate WebP size client-side (mirrors server: max 1600px, quality ~0.72 for large files). */
    function estimateWebpBytes(file, maxDimension, quality) {
        return new Promise(function (resolve) {
            if (!file || !file.type || !file.type.startsWith('image/')) {
                resolve(null);
                return;
            }

            var url = URL.createObjectURL(file);
            var img = new Image();
            img.onload = function () {
                try {
                    var w = img.naturalWidth || img.width;
                    var h = img.naturalHeight || img.height;
                    if (w > maxDimension || h > maxDimension) {
                        var scale = Math.min(maxDimension / w, maxDimension / h);
                        w = Math.max(1, Math.round(w * scale));
                        h = Math.max(1, Math.round(h * scale));
                    }

                    var canvas = document.createElement('canvas');
                    canvas.width = w;
                    canvas.height = h;
                    var ctx = canvas.getContext('2d');
                    if (!ctx) {
                        URL.revokeObjectURL(url);
                        resolve(null);
                        return;
                    }
                    ctx.drawImage(img, 0, 0, w, h);

                    if (canvas.toBlob) {
                        canvas.toBlob(function (blob) {
                            URL.revokeObjectURL(url);
                            resolve(blob ? blob.size : null);
                        }, 'image/webp', quality);
                    } else {
                        // Fallback: dataURL length approximation
                        var dataUrl = canvas.toDataURL('image/webp', quality);
                        var approx = Math.round((dataUrl.length - 'data:image/webp;base64,'.length) * 0.75);
                        URL.revokeObjectURL(url);
                        resolve(approx);
                    }
                } catch (_) {
                    URL.revokeObjectURL(url);
                    resolve(null);
                }
            };
            img.onerror = function () {
                URL.revokeObjectURL(url);
                resolve(null);
            };
            img.src = url;
        });
    }

    function closeUploadModal() {
        uploadModalCtx = null;
        if (!uploadModal) return;
        uploadModal.classList.add('is-hidden');
        uploadModal.setAttribute('aria-hidden', 'true');
        document.body.classList.remove('admin-modal-open');
    }

    function openUploadModal(ctx) {
        uploadModalCtx = ctx;
        if (!uploadModal) {
            ctx.apply(true);
            return;
        }
        var root = ctx.root;
        var file = ctx.file;
        var titleEl = document.getElementById('admin-upload-modal-title');
        var textEl = document.getElementById('admin-upload-modal-text');
        var btnCompress = uploadModal.querySelector('[data-upload-modal-compress]');
        var btnKeep = uploadModal.querySelector('[data-upload-modal-keep]');
        if (titleEl) titleEl.textContent = root.getAttribute('data-upload-modal-title') || '';
        if (textEl) {
            textEl.textContent = (root.getAttribute('data-upload-modal-text') || '')
                .replace('{size}', formatUploadSize(file.size));
        }
        if (btnCompress) btnCompress.textContent = root.getAttribute('data-upload-compress') || 'Compress';
        if (btnKeep) btnKeep.textContent = root.getAttribute('data-upload-keep') || 'Keep original';
        uploadModal.querySelectorAll('[data-upload-modal-cancel]').forEach(function (btn) {
            if (btn.tagName === 'BUTTON') {
                btn.textContent = root.getAttribute('data-upload-cancel') || 'Cancel';
            }
        });
        uploadModal.classList.remove('is-hidden');
        uploadModal.setAttribute('aria-hidden', 'false');
        document.body.classList.add('admin-modal-open');
    }

    if (uploadModal) {
        uploadModal.querySelector('[data-upload-modal-compress]')?.addEventListener('click', function () {
            if (uploadModalCtx) uploadModalCtx.apply(true);
        });
        uploadModal.querySelector('[data-upload-modal-keep]')?.addEventListener('click', function () {
            if (uploadModalCtx) uploadModalCtx.apply(false);
        });
        uploadModal.querySelectorAll('[data-upload-modal-cancel]').forEach(function (el) {
            el.addEventListener('click', closeUploadModal);
        });
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && uploadModalCtx) closeUploadModal();
        });
    }

    document.querySelectorAll('[data-upload]').forEach(function (root) {
        var dropzone = root.querySelector('[data-upload-dropzone]');
        var input = root.querySelector('[data-upload-input]');
        var hidden = root.querySelector('[data-upload-hidden]');
        var clearBtn = root.querySelector('[data-upload-clear]');
        var choiceEl = root.querySelector('[data-upload-choice]');
        var compressFlag = root.querySelector('[data-upload-compress-flag]');
        var isDocument = root.getAttribute('data-upload-kind') === 'document';
        var warnBytes = parseInt(root.getAttribute('data-upload-warn-bytes') || '1048576', 10);
        var maxBytes = parseInt(root.getAttribute('data-upload-max-bytes') || '15728640', 10);

        function setCompress(value) {
            if (compressFlag) compressFlag.value = value ? 'true' : 'false';
        }

        function setChoiceLabel(compress, file, estimatedBytes) {
            if (!choiceEl || !file) {
                if (choiceEl) {
                    choiceEl.classList.add('is-hidden');
                    choiceEl.textContent = '';
                }
                return;
            }

            var from = formatUploadSize(file.size);
            if (compress) {
                if (estimatedBytes == null) {
                    choiceEl.textContent = (root.getAttribute('data-upload-compress') || 'Compress')
                        + ' · ' + from + ' → …';
                } else {
                    var to = formatUploadSize(estimatedBytes);
                    var template = root.getAttribute('data-upload-size-reduced') || '{from} → {to}';
                    choiceEl.textContent = (root.getAttribute('data-upload-compress') || 'Compress')
                        + ' · ' + template.replace('{from}', from).replace('{to}', to);
                }
            } else {
                choiceEl.textContent = (root.getAttribute('data-upload-keep') || 'Original') + ' · ' + from;
            }

            choiceEl.classList.toggle('is-compress', compress);
            choiceEl.classList.toggle('is-keep', !compress);
            choiceEl.classList.remove('is-hidden');
        }

        function setPreview(file) {
            if (!dropzone || !file) return;
            dropzone.classList.add('has-file');
            if (clearBtn) clearBtn.classList.remove('is-hidden');

            if (isDocument) {
                dropzone.innerHTML = '';
                var doc = document.createElement('div');
                doc.className = 'admin-upload-doc';
                doc.setAttribute('data-upload-preview-wrap', '');
                doc.innerHTML = '<span class="admin-upload-doc-icon" aria-hidden="true">FILE</span><span class="admin-upload-doc-name"></span>';
                doc.querySelector('.admin-upload-doc-name').textContent = file.name;
                dropzone.appendChild(doc);
                dropzone.appendChild(input);
            } else if (file.type.startsWith('image/')) {
                var reader = new FileReader();
                reader.onload = function (ev) {
                    dropzone.innerHTML = '';
                    var wrap = document.createElement('div');
                    wrap.className = 'admin-upload-preview';
                    wrap.setAttribute('data-upload-preview-wrap', '');
                    var img = document.createElement('img');
                    img.className = 'admin-upload-preview-img';
                    img.setAttribute('data-upload-preview', '');
                    img.src = ev.target.result;
                    img.alt = '';
                    wrap.appendChild(img);
                    dropzone.appendChild(wrap);
                    dropzone.appendChild(input);
                };
                reader.readAsDataURL(file);
            } else {
                dropzone.appendChild(input);
            }
        }

        function applyFile(file, compress) {
            if (!input || !file) return;
            var dt = new DataTransfer();
            dt.items.add(file);
            input.files = dt.files;
            setCompress(compress);
            setPreview(file);
            closeUploadModal();

            if (compress) {
                setChoiceLabel(true, file, null);
                var quality = file.size > warnBytes ? 0.72 : 0.82;
                estimateWebpBytes(file, 1600, quality).then(function (estimated) {
                    setChoiceLabel(true, file, estimated);
                });
            } else {
                setChoiceLabel(false, file);
            }
        }

        function handleSelectedFile(file) {
            if (!file) return;

            if (!isDocument && file.size > maxBytes) {
                alert((root.getAttribute('data-upload-too-large') || 'File is too large.') + ' (' + formatUploadSize(file.size) + ')');
                if (input) input.value = '';
                return;
            }

            if (!isDocument && file.type.startsWith('image/') && file.size > warnBytes) {
                if (input) input.value = '';
                openUploadModal({
                    root: root,
                    file: file,
                    apply: function (compress) { applyFile(file, compress); }
                });
                return;
            }

            setCompress(false);
            setChoiceLabel(false, null);
            if (choiceEl) choiceEl.classList.add('is-hidden');
            setPreview(file);
        }

        function clearPreview() {
            if (hidden) hidden.value = '';
            if (input) input.value = '';
            setCompress(true);
            if (clearBtn) clearBtn.classList.add('is-hidden');
            if (choiceEl) {
                choiceEl.classList.add('is-hidden');
                choiceEl.textContent = '';
            }
            if (!dropzone) return;
            dropzone.classList.remove('has-file');
            dropzone.innerHTML = '<div class="admin-upload-empty" data-upload-preview-wrap=""><span class="admin-upload-icon" aria-hidden="true"><svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="17 8 12 3 7 8"/><line x1="12" y1="3" x2="12" y2="15"/></svg></span><strong>Drop file</strong><span>or click to browse</span></div>';
            dropzone.appendChild(input);
        }

        input?.addEventListener('change', function () {
            var file = input.files && input.files[0];
            if (file) handleSelectedFile(file);
        });

        clearBtn?.addEventListener('click', clearPreview);

        ['dragenter', 'dragover'].forEach(function (evt) {
            dropzone?.addEventListener(evt, function (e) {
                e.preventDefault();
                dropzone.classList.add('is-dragover');
            });
        });

        ['dragleave', 'drop'].forEach(function (evt) {
            dropzone?.addEventListener(evt, function (e) {
                e.preventDefault();
                dropzone.classList.remove('is-dragover');
            });
        });

        dropzone?.addEventListener('drop', function (e) {
            var file = e.dataTransfer?.files?.[0];
            if (!file) return;
            handleSelectedFile(file);
        });
    });
})();
