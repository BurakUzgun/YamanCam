// Aranabilir combobox: "searchable-select" sinifli <select> elemanlarini,
// yazilan metnin secenek icinde HERHANGI bir yerde gectigi aramali bir kutuya cevirir.
//
// - Orijinal <select> DOM'da kalir (gizlenir); form ayni name ile gonderilir.
// - Secim yapilinca select'e "change" olayi gonderilir; mevcut handler'lar aynen calisir.
// - Kod icinden select.value = x atamasi yapilirsa gorunen metin otomatik guncellenir.
// - Sonradan eklenen satirlardaki select'ler MutationObserver ile otomatik baglanir.
(function () {
    'use strict';

    var SELECTOR = 'select.searchable-select';
    var MAX_RESULTS = 200;
    var valueDesc = Object.getOwnPropertyDescriptor(HTMLSelectElement.prototype, 'value');
    var indexDesc = Object.getOwnPropertyDescriptor(HTMLSelectElement.prototype, 'selectedIndex');

    var dropdown = null;   // tum sayfa icin tek acilir liste
    var current = null;    // o an acik olan bilesen

    function normalize(text) {
        return (text || '').toLocaleLowerCase('tr-TR').replace(/\s+/g, ' ').trim();
    }

    function ensureDropdown() {
        if (dropdown) return dropdown;
        dropdown = document.createElement('div');
        dropdown.className = 'ss-dropdown';
        dropdown.setAttribute('role', 'listbox');
        dropdown.style.display = 'none';
        // Tiklama input'un blur'unu tetiklemesin
        dropdown.addEventListener('mousedown', function (e) { e.preventDefault(); });
        dropdown.addEventListener('click', function (e) {
            var item = e.target.closest('.ss-item');
            if (item && current) current.choose(parseInt(item.getAttribute('data-index'), 10));
        });
        document.body.appendChild(dropdown);

        window.addEventListener('resize', function () { if (current) current.position(); });
        window.addEventListener('scroll', function (e) {
            if (!current) return;
            if (e.target === dropdown || dropdown.contains(e.target)) return;
            current.position();
        }, true);
        return dropdown;
    }

    function SearchableSelect(select) {
        var self = this;
        this.select = select;
        this.items = [];
        this.results = [];
        this.active = -1;

        this.input = document.createElement('input');
        this.input.type = 'text';
        this.input.autocomplete = 'off';
        this.input.spellcheck = false;
        this.input.className = 'form-control ss-input' + (select.classList.contains('form-select-sm') ? ' form-control-sm' : '');
        this.input.setAttribute('role', 'combobox');
        this.input.setAttribute('aria-autocomplete', 'list');

        select.classList.add('ss-hidden');
        select.setAttribute('tabindex', '-1');
        select.parentNode.insertBefore(this.input, select.nextSibling);

        // Kod icinden yapilan value / selectedIndex atamalarinda gorunen metni esitle
        Object.defineProperty(select, 'value', {
            configurable: true,
            get: function () { return valueDesc.get.call(this); },
            set: function (v) { valueDesc.set.call(this, v); self.sync(); }
        });
        Object.defineProperty(select, 'selectedIndex', {
            configurable: true,
            get: function () { return indexDesc.get.call(this); },
            set: function (v) { indexDesc.set.call(this, v); self.sync(); }
        });

        select.addEventListener('change', function () { self.sync(); });
        select.form && select.form.addEventListener('reset', function () { setTimeout(function () { self.sync(); }, 0); });

        this.input.addEventListener('focus', function () {
            self.input.select();
            self.open('');
        });
        this.input.addEventListener('click', function () {
            if (current !== self) self.open('');
        });
        this.input.addEventListener('input', function () {
            self.open(self.input.value);
        });
        this.input.addEventListener('keydown', function (e) { self.onKeyDown(e); });
        this.input.addEventListener('blur', function () {
            self.close();
            self.sync();
        });

        this.sync();
    }

    SearchableSelect.prototype.readItems = function () {
        var opts = this.select.options;
        var list = [];
        for (var i = 0; i < opts.length; i++) {
            list.push({ index: i, text: opts[i].text, key: normalize(opts[i].text), placeholder: opts[i].value === '' || opts[i].value === '0' });
        }
        this.items = list;
    };

    SearchableSelect.prototype.sync = function () {
        var opt = this.select.options[indexDesc.get.call(this.select)];
        var isPlaceholder = !opt || opt.value === '' || opt.value === '0';
        this.input.value = isPlaceholder ? '' : opt.text;
        this.input.placeholder = opt && isPlaceholder ? opt.text : '';
        this.input.disabled = this.select.disabled;
        this.input.title = isPlaceholder ? '' : opt.text;
    };

    SearchableSelect.prototype.filter = function (query) {
        var terms = normalize(query).split(' ').filter(Boolean);
        var out = [];
        for (var i = 0; i < this.items.length && out.length < MAX_RESULTS; i++) {
            var it = this.items[i];
            if (terms.length === 0) { out.push(it); continue; }
            if (it.placeholder) continue;
            var ok = true;
            for (var t = 0; t < terms.length; t++) {
                if (it.key.indexOf(terms[t]) === -1) { ok = false; break; }
            }
            if (ok) out.push(it);
        }
        return out;
    };

    SearchableSelect.prototype.open = function (query) {
        if (this.select.disabled) return;
        var dd = ensureDropdown();
        if (current && current !== this) current.close();
        current = this;
        this.readItems();
        this.results = this.filter(query);

        var selIdx = indexDesc.get.call(this.select);
        this.active = 0;
        if (!query) {
            for (var r = 0; r < this.results.length; r++) {
                if (this.results[r].index === selIdx) { this.active = r; break; }
            }
        }
        this.render(query);
        dd.style.display = 'block';
        this.position();
        this.input.setAttribute('aria-expanded', 'true');
        this.scrollActiveIntoView();
    };

    SearchableSelect.prototype.render = function (query) {
        var dd = dropdown;
        dd.innerHTML = '';
        if (this.results.length === 0) {
            var empty = document.createElement('div');
            empty.className = 'ss-empty';
            empty.textContent = 'Sonuç bulunamadı';
            dd.appendChild(empty);
            this.active = -1;
            return;
        }
        var terms = normalize(query).split(' ').filter(Boolean);
        var frag = document.createDocumentFragment();
        for (var i = 0; i < this.results.length; i++) {
            var it = this.results[i];
            var div = document.createElement('div');
            div.className = 'ss-item' + (i === this.active ? ' active' : '') + (it.placeholder ? ' ss-placeholder' : '');
            div.setAttribute('data-index', it.index);
            div.setAttribute('role', 'option');
            appendHighlighted(div, it.text, terms);
            frag.appendChild(div);
        }
        dd.appendChild(frag);
    };

    // Eslesen kisimlari kalin gosterir (metin DOM'a textContent ile eklenir, HTML olarak degil)
    function appendHighlighted(el, text, terms) {
        if (!terms.length) { el.textContent = text; return; }
        var lower = text.toLocaleLowerCase('tr-TR');
        // tr-TR kucuk harf donusumu uzunlugu degistirirse vurgulamayi atla
        if (lower.length !== text.length) { el.textContent = text; return; }
        var marks = new Array(text.length);
        terms.forEach(function (t) {
            var from = 0, pos;
            while (t && (pos = lower.indexOf(t, from)) !== -1) {
                for (var k = pos; k < pos + t.length; k++) marks[k] = true;
                from = pos + t.length;
            }
        });
        var buf = '', bold = false;
        function flush() {
            if (!buf) return;
            if (bold) { var m = document.createElement('mark'); m.textContent = buf; el.appendChild(m); }
            else el.appendChild(document.createTextNode(buf));
            buf = '';
        }
        for (var i = 0; i < text.length; i++) {
            var b = !!marks[i];
            if (b !== bold) { flush(); bold = b; }
            buf += text[i];
        }
        flush();
    }

    SearchableSelect.prototype.position = function () {
        if (!dropdown || current !== this) return;
        var rect = this.input.getBoundingClientRect();
        if (rect.width === 0 && rect.height === 0) { this.close(); return; }
        var width = Math.max(rect.width, 320);
        var left = Math.min(rect.left, window.innerWidth - width - 8);
        dropdown.style.width = width + 'px';
        dropdown.style.left = Math.max(8, left) + 'px';

        var spaceBelow = window.innerHeight - rect.bottom - 8;
        var spaceAbove = rect.top - 8;
        var maxH = 320;
        if (spaceBelow < 200 && spaceAbove > spaceBelow) {
            var h = Math.min(maxH, spaceAbove);
            dropdown.style.maxHeight = h + 'px';
            dropdown.style.top = 'auto';
            dropdown.style.bottom = (window.innerHeight - rect.top + 2) + 'px';
        } else {
            dropdown.style.maxHeight = Math.min(maxH, Math.max(spaceBelow, 120)) + 'px';
            dropdown.style.bottom = 'auto';
            dropdown.style.top = (rect.bottom + 2) + 'px';
        }
    };

    SearchableSelect.prototype.close = function () {
        if (current !== this) return;
        dropdown.style.display = 'none';
        dropdown.innerHTML = '';
        current = null;
        this.input.setAttribute('aria-expanded', 'false');
    };

    SearchableSelect.prototype.setActive = function (i) {
        if (!this.results.length) return;
        if (i < 0) i = 0;
        if (i >= this.results.length) i = this.results.length - 1;
        var nodes = dropdown.querySelectorAll('.ss-item');
        if (nodes[this.active]) nodes[this.active].classList.remove('active');
        this.active = i;
        if (nodes[i]) nodes[i].classList.add('active');
        this.scrollActiveIntoView();
    };

    SearchableSelect.prototype.scrollActiveIntoView = function () {
        var node = dropdown && dropdown.querySelectorAll('.ss-item')[this.active];
        if (!node) return;
        var top = node.offsetTop, bottom = top + node.offsetHeight;
        if (top < dropdown.scrollTop) dropdown.scrollTop = top;
        else if (bottom > dropdown.scrollTop + dropdown.clientHeight) dropdown.scrollTop = bottom - dropdown.clientHeight;
    };

    SearchableSelect.prototype.choose = function (optionIndex) {
        var changed = indexDesc.get.call(this.select) !== optionIndex;
        indexDesc.set.call(this.select, optionIndex);
        this.close();
        this.sync();
        if (changed) {
            this.select.dispatchEvent(new Event('input', { bubbles: true }));
            this.select.dispatchEvent(new Event('change', { bubbles: true }));
        }
    };

    SearchableSelect.prototype.onKeyDown = function (e) {
        var isOpen = current === this;
        switch (e.key) {
            case 'ArrowDown':
                e.preventDefault();
                if (!isOpen) this.open(''); else this.setActive(this.active + 1);
                break;
            case 'ArrowUp':
                e.preventDefault();
                if (isOpen) this.setActive(this.active - 1);
                break;
            case 'PageDown':
                if (isOpen) { e.preventDefault(); this.setActive(this.active + 10); }
                break;
            case 'PageUp':
                if (isOpen) { e.preventDefault(); this.setActive(this.active - 10); }
                break;
            case 'Enter':
                // Enter formu gondermesin; acik listede aktif ogeyi sec
                e.preventDefault();
                if (isOpen && this.active >= 0 && this.results[this.active]) {
                    this.choose(this.results[this.active].index);
                } else if (!isOpen) {
                    this.open('');
                }
                break;
            case 'Tab':
                // Kullanici yazip Tab'a basarsa aktif ogeyi sec ve sonraki alana gec
                if (isOpen && this.input.value !== '' && this.active >= 0 && this.results[this.active]
                    && this.input.value !== (this.select.options[indexDesc.get.call(this.select)] || {}).text) {
                    this.choose(this.results[this.active].index);
                } else if (isOpen) {
                    this.close();
                }
                break;
            case 'Escape':
                if (isOpen) {
                    e.preventDefault();
                    this.close();
                    this.sync();
                    this.input.select();
                }
                break;
        }
    };

    function init(select) {
        if (!select || select.__searchable) return select && select.__searchable;
        select.__searchable = new SearchableSelect(select);
        return select.__searchable;
    }

    function initAll(root) {
        (root || document).querySelectorAll(SELECTOR).forEach(init);
    }

    function start() {
        initAll(document);
        new MutationObserver(function (mutations) {
            mutations.forEach(function (m) {
                m.addedNodes.forEach(function (node) {
                    if (node.nodeType !== 1) return;
                    if (node.matches && node.matches(SELECTOR)) init(node);
                    else if (node.querySelectorAll) initAll(node);
                });
                m.removedNodes.forEach(function (node) {
                    if (current && node.nodeType === 1 && node.contains(current.input)) current.close();
                });
            });
        }).observe(document.body, { childList: true, subtree: true });
    }

    window.SearchableSelect = { init: init, initAll: initAll };

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', start);
    else start();
})();
