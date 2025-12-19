/**
 * √œ«… «· ÕœÌœ «·„ ⁄œœ «·„Õ”‰… ··Â« ›
 * Enhanced Multi-Select Component for Mobile
 */

class MobileMultiSelect {
    constructor(element, options = {}) {
        this.element = element;
        this.options = {
            placeholder: '«Œ — «·⁄‰«’—...',
            searchPlaceholder: '«·»ÕÀ...',
            noResultsText: '·«  ÊÃœ ‰ «∆Ã',
            clearAllText: '„”Õ «·ﬂ·',
            maxDisplayTags: 3,
            enableSearch: true,
            enableClearAll: true,
            ...options
        };

        this.selectedValues = [];
        this.allOptions = [];
        this.filteredOptions = [];
        this.isOpen = false;

        this.init();
    }

    init() {
        this.parseOriginalSelect();
        this.createMultiSelectHTML();
        this.bindEvents();
        this.updateDisplay();
    }

    parseOriginalSelect() {
        // «” Œ—«Ã «·ŒÌ«—«  „‰ select «·√’·Ì
        const options = this.element.querySelectorAll('option');
        this.allOptions = Array.from(options).map(option => ({
            value: option.value,
            text: option.textContent.trim(),
            selected: option.selected
        })).filter(opt => opt.value); //  Ã«Â· «·ŒÌ«—«  «·›«—€…

        this.filteredOptions = [...this.allOptions];
        this.selectedValues = this.allOptions.filter(opt => opt.selected).map(opt => opt.value);

        // ≈Œ›«¡ select «·√’·Ì
        this.element.style.display = 'none';
    }

    createMultiSelectHTML() {
        const container = document.createElement('div');
        container.className = 'multi-select-container';

        //  ÕœÌœ « Ã«Â «·‰’
        const isRTL = document.documentElement.dir === 'rtl' ||
            document.body.dir === 'rtl' ||
            getComputedStyle(document.documentElement).direction === 'rtl';

        if (isRTL) {
            container.setAttribute('dir', 'rtl');
        }

        container.innerHTML = `
            <div class="multi-select-dropdown" tabindex="0" role="button" aria-haspopup="listbox">
                <div class="multi-select-selected">
                    <span class="multi-select-placeholder">${this.options.placeholder}</span>
                </div>
                <div class="multi-select-counter" style="display: none;">0</div>
                <i class="fas fa-chevron-down multi-select-arrow"></i>
            </div>
            <div class="multi-select-options" role="listbox">
                ${this.options.enableSearch ? `
                    <div class="multi-select-search">
                        <input type="text" placeholder="${this.options.searchPlaceholder}" autocomplete="off">
                    </div>
                ` : ''}
                <div class="multi-select-options-list"></div>
                ${this.options.enableClearAll ? `
                    <div class="multi-select-footer" style="padding: 0.5rem; border-top: 1px solid #e9ecef; display: none;">
                        <button type="button" class="multi-select-clear-all">${this.options.clearAllText}</button>
                    </div>
                ` : ''}
            </div>
        `;

        // ≈œ—«Ã «·Õ«ÊÌ… »⁄œ select «·√’·Ì
        this.element.parentNode.insertBefore(container, this.element.nextSibling);

        // Õ›Ÿ «·„—«Ã⁄
        this.container = container;
        this.dropdown = container.querySelector('.multi-select-dropdown');
        this.selectedContainer = container.querySelector('.multi-select-selected');
        this.optionsContainer = container.querySelector('.multi-select-options');
        this.optionsList = container.querySelector('.multi-select-options-list');
        this.searchInput = container.querySelector('.multi-select-search input');
        this.clearAllBtn = container.querySelector('.multi-select-clear-all');
        this.counter = container.querySelector('.multi-select-counter');
        this.arrow = container.querySelector('.multi-select-arrow');
        this.footer = container.querySelector('.multi-select-footer');
    }

    bindEvents() {
        // › Õ/≈€·«ﬁ «·ﬁ«∆„…
        this.dropdown.addEventListener('click', (e) => {
            e.stopPropagation();
            this.toggle();
        });

        // «·»ÕÀ
        if (this.searchInput) {
            this.searchInput.addEventListener('input', (e) => {
                this.filterOptions(e.target.value);
            });

            this.searchInput.addEventListener('click', (e) => {
                e.stopPropagation();
            });
        }

        // „”Õ «·ﬂ·
        if (this.clearAllBtn) {
            this.clearAllBtn.addEventListener('click', (e) => {
                e.stopPropagation();
                this.clearAll();
            });
        }

        // ≈€·«ﬁ ⁄‰œ «·‰ﬁ— Œ«—Ã «·⁄‰’—
        document.addEventListener('click', (e) => {
            if (!this.container.contains(e.target)) {
                this.close();
            }
        });

        // œ⁄„ ·ÊÕ… «·„›« ÌÕ
        this.dropdown.addEventListener('keydown', (e) => {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                this.toggle();
            } else if (e.key === 'Escape') {
                this.close();
            }
        });

        // „‰⁄ ≈€·«ﬁ «·ﬁ«∆„… ⁄‰œ «·‰ﬁ— œ«Œ·Â«
        this.optionsContainer.addEventListener('click', (e) => {
            e.stopPropagation();
        });
    }

    renderOptions() {
        this.optionsList.innerHTML = '';

        if (this.filteredOptions.length === 0) {
            this.optionsList.innerHTML = `
                <div class="multi-select-no-results">
                    <i class="fas fa-search me-2"></i>
                    ${this.options.noResultsText}
                </div>
            `;
            return;
        }

        this.filteredOptions.forEach(option => {
            const optionElement = document.createElement('div');
            optionElement.className = `multi-select-option ${this.selectedValues.includes(option.value) ? 'selected' : ''}`;
            optionElement.setAttribute('data-value', option.value);
            optionElement.setAttribute('role', 'option');
            optionElement.setAttribute('tabindex', '0');

            optionElement.innerHTML = `
                <div class="multi-select-checkbox"></div>
                <span class="multi-select-option-text">${option.text}</span>
            `;

            optionElement.addEventListener('click', () => {
                this.toggleOption(option.value);
            });

            optionElement.addEventListener('keydown', (e) => {
                if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    this.toggleOption(option.value);
                }
            });

            this.optionsList.appendChild(optionElement);
        });
    }

    toggleOption(value) {
        const index = this.selectedValues.indexOf(value);

        if (index > -1) {
            // ≈“«·… «·«Œ Ì«—
            this.selectedValues.splice(index, 1);
        } else {
            // ≈÷«›… «·«Œ Ì«—
            this.selectedValues.push(value);
        }

        this.updateOriginalSelect();
        this.updateDisplay();
        this.renderOptions();

        // ≈—”«· ÕœÀ «· €ÌÌ—
        this.element.dispatchEvent(new Event('change', { bubbles: true }));
    }

    updateOriginalSelect() {
        //  ÕœÌÀ select «·√’·Ì
        const options = this.element.querySelectorAll('option');
        options.forEach(option => {
            option.selected = this.selectedValues.includes(option.value);
        });

        //  ÕœÌÀ hidden field ≈–« ÊÃœ (·· Ê«›ﬁ „⁄ «·√‰Ÿ„… «·„ÊÃÊœ…)
        const hiddenField = document.querySelector(`input[type="hidden"][name="${this.element.name}"]`);
        if (hiddenField) {
            hiddenField.value = this.selectedValues.join(',');
        }

        //  ÕœÌÀ validation state
        this.updateValidationState();
    }

    updateDisplay() {
        const selectedOptions = this.allOptions.filter(opt =>
            this.selectedValues.includes(opt.value)
        );

        this.selectedContainer.innerHTML = '';

        if (selectedOptions.length === 0) {
            // ⁄—÷ placeholder
            const placeholder = document.createElement('span');
            placeholder.className = 'multi-select-placeholder';
            placeholder.textContent = this.options.placeholder;
            this.selectedContainer.appendChild(placeholder);

            this.counter.style.display = 'none';
            if (this.footer) this.footer.style.display = 'none';
        } else {
            // ⁄—÷ «·⁄‰«’— «·„Œ «—…
            const displayCount = Math.min(selectedOptions.length, this.options.maxDisplayTags);

            for (let i = 0; i < displayCount; i++) {
                const tag = this.createTag(selectedOptions[i]);
                this.selectedContainer.appendChild(tag);
            }

            // ⁄—÷ «·⁄œ«œ ≈–« ﬂ«‰ Â‰«ﬂ ⁄‰«’— √ﬂÀ—
            if (selectedOptions.length > this.options.maxDisplayTags) {
                this.counter.textContent = `+${selectedOptions.length - this.options.maxDisplayTags}`;
                this.counter.style.display = 'inline-block';
            } else {
                this.counter.style.display = 'none';
            }

            if (this.footer) this.footer.style.display = 'block';
        }
    }

    createTag(option) {
        const tag = document.createElement('div');
        tag.className = 'multi-select-tag';
        tag.innerHTML = `
            <span>${option.text}</span>
            <button type="button" class="remove-tag" title="≈“«·…" data-value="${option.value}">
                <i class="fas fa-times"></i>
            </button>
        `;

        const removeBtn = tag.querySelector('.remove-tag');
        removeBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            this.toggleOption(option.value);
        });

        return tag;
    }

    filterOptions(searchTerm) {
        const term = searchTerm.toLowerCase().trim();

        if (!term) {
            this.filteredOptions = [...this.allOptions];
        } else {
            this.filteredOptions = this.allOptions.filter(option =>
                option.text.toLowerCase().includes(term)
            );
        }

        this.renderOptions();
    }

    toggle() {
        if (this.isOpen) {
            this.close();
        } else {
            this.open();
        }
    }

    open() {
        if (this.isOpen) return;

        this.isOpen = true;
        this.dropdown.classList.add('active');
        this.optionsContainer.classList.add('show');
        this.renderOptions();

        //  —ﬂÌ“ «·»ÕÀ ≈–« ﬂ«‰ „ «Õ«
        if (this.searchInput) {
            setTimeout(() => {
                this.searchInput.focus();
            }, 100);
        }

        // ≈—”«· ÕœÀ «·› Õ
        this.element.dispatchEvent(new CustomEvent('multiselect:open'));
    }

    close() {
        if (!this.isOpen) return;

        this.isOpen = false;
        this.dropdown.classList.remove('active');
        this.optionsContainer.classList.remove('show');

        // „”Õ «·»ÕÀ
        if (this.searchInput) {
            this.searchInput.value = '';
            this.filterOptions('');
        }

        // ≈—”«· ÕœÀ «·≈€·«ﬁ
        this.element.dispatchEvent(new CustomEvent('multiselect:close'));
    }

    clearAll() {
        this.selectedValues = [];
        this.updateOriginalSelect();
        this.updateDisplay();
        this.renderOptions();

        // ≈—”«· ÕœÀ «· €ÌÌ—
        this.element.dispatchEvent(new Event('change', { bubbles: true }));
        this.element.dispatchEvent(new CustomEvent('multiselect:clear'));
    }

    // œÊ«· ⁄«„… ·· Õﬂ„ „‰ «·Œ«—Ã
    setValue(values) {
        this.selectedValues = Array.isArray(values) ? values : [values];
        this.updateOriginalSelect();
        this.updateDisplay();
        this.renderOptions();
    }

    getValue() {
        return this.selectedValues;
    }

    addOption(value, text) {
        this.allOptions.push({ value, text, selected: false });
        this.filteredOptions = [...this.allOptions];

        // ≈÷«›… option ··‹ select «·√’·Ì
        const option = document.createElement('option');
        option.value = value;
        option.textContent = text;
        this.element.appendChild(option);

        this.renderOptions();
    }

    removeOption(value) {
        this.allOptions = this.allOptions.filter(opt => opt.value !== value);
        this.filteredOptions = this.filteredOptions.filter(opt => opt.value !== value);
        this.selectedValues = this.selectedValues.filter(val => val !== value);

        // ≈“«·… option „‰ «·‹ select «·√’·Ì
        const option = this.element.querySelector(`option[value="${value}"]`);
        if (option) option.remove();

        this.updateDisplay();
        this.renderOptions();
    }

    disable() {
        this.container.classList.add('multi-select-disabled');
        this.dropdown.setAttribute('tabindex', '-1');
        this.element.disabled = true;
    }

    enable() {
        this.container.classList.remove('multi-select-disabled');
        this.dropdown.setAttribute('tabindex', '0');
        this.element.disabled = false;
    }

    updateValidationState() {
        // ≈“«·… Ã„Ì⁄ Õ«·«  «· Õﬁﬁ «·”«»ﬁ…
        this.container.classList.remove('multi-select-error', 'multi-select-success', 'multi-select-required');

        // «· Õﬁﬁ „‰ «·ÕﬁÊ· «·„ÿ·Ê»…
        if (this.element.hasAttribute('required') || this.element.hasAttribute('data-required')) {
            if (this.selectedValues.length === 0) {
                this.container.classList.add('multi-select-error', 'multi-select-required');
            } else {
                this.container.classList.add('multi-select-success');
            }
        }

        // «· Õﬁﬁ „‰ jQuery Validation ≈–« ﬂ«‰ „ «Õ«
        if (typeof $ !== 'undefined' && $.fn.valid) {
            const $element = $(this.element);
            if ($element.length && $element.closest('form').length) {
                const isValid = $element.valid();
                if (isValid) {
                    this.container.classList.add('multi-select-success');
                    this.container.classList.remove('multi-select-error');
                } else {
                    this.container.classList.add('multi-select-error');
                    this.container.classList.remove('multi-select-success');
                }
            }
        }
    }

    // œ«·… ·· Õﬁﬁ „‰ ’Õ… «·«Œ Ì«—
    validate() {
        if (this.element.hasAttribute('required') || this.element.hasAttribute('data-required')) {
            return this.selectedValues.length > 0;
        }
        return true;
    }

    // œ«·… ·⁄—÷ —”«·… Œÿ√
    showError(message) {
        this.container.classList.add('multi-select-error');

        // «·»ÕÀ ⁄‰ ⁄‰’— ⁄—÷ «·Œÿ√
        let errorElement = this.container.nextElementSibling;
        if (errorElement && errorElement.classList.contains('text-danger')) {
            errorElement.textContent = message;
            errorElement.style.display = 'block';
        } else {
            // ≈‰‘«¡ ⁄‰’— Œÿ√ ÃœÌœ
            errorElement = document.createElement('div');
            errorElement.className = 'multi-select-error-message text-danger';
            errorElement.innerHTML = `<i class="fas fa-exclamation-circle"></i> ${message}`;
            this.container.parentNode.insertBefore(errorElement, this.container.nextSibling);
        }
    }

    // œ«·… ·≈Œ›«¡ —”«·… «·Œÿ√
    hideError() {
        this.container.classList.remove('multi-select-error');

        const errorElement = this.container.nextElementSibling;
        if (errorElement && (errorElement.classList.contains('text-danger') || errorElement.classList.contains('multi-select-error-message'))) {
            errorElement.style.display = 'none';
        }
    }

    destroy() {
        if (this.container && this.container.parentNode) {
            this.container.parentNode.removeChild(this.container);
        }
        this.element.style.display = '';
    }
}

// œ«·… „”«⁄œ… · ÂÌ∆… Ã„Ì⁄ ⁄‰«’— «· ÕœÌœ «·„ ⁄œœ
function initializeMultiSelects() {
    const selects = document.querySelectorAll('select[multiple].mobile-multi-select');
    const instances = [];

    selects.forEach(select => {
        const options = {
            placeholder: select.getAttribute('data-placeholder') || '«Œ — «·⁄‰«’—...',
            searchPlaceholder: select.getAttribute('data-search-placeholder') || '«·»ÕÀ...',
            noResultsText: select.getAttribute('data-no-results') || '·«  ÊÃœ ‰ «∆Ã',
            clearAllText: select.getAttribute('data-clear-all') || '„”Õ «·ﬂ·',
            maxDisplayTags: parseInt(select.getAttribute('data-max-tags')) || 3,
            enableSearch: select.getAttribute('data-enable-search') !== 'false',
            enableClearAll: select.getAttribute('data-enable-clear-all') !== 'false'
        };

        const instance = new MobileMultiSelect(select, options);
        instances.push(instance);

        // Õ›Ÿ «·„—Ã⁄ ›Ì «·⁄‰’— ··Ê’Ê· ≈·ÌÂ ·«Õﬁ«
        select.multiSelectInstance = instance;
    });

    return instances;
}

//  ÂÌ∆…  ·ﬁ«∆Ì… ⁄‰œ  Õ„Ì· «·’›Õ…
document.addEventListener('DOMContentLoaded', function () {
    initializeMultiSelects();
});

//  ’œÌ— ··«” Œœ«„ «·⁄«„
window.MobileMultiSelect = MobileMultiSelect;
window.initializeMultiSelects = initializeMultiSelects;