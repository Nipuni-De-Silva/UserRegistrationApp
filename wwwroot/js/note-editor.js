// Note Editor JavaScript Functions - Enhanced with debugging and error handling
console.log('Note editor JavaScript file loading...');

// Enhanced getEditorContent with debugging
window.getEditorContent = (element) => {
    console.log('getEditorContent called with element:', element);
    try {
        if (!element) {
            console.warn('getEditorContent: Element is null or undefined');
            return '';
        }
        
        if (!element.innerHTML) {
            console.warn('getEditorContent: Element has no innerHTML');
            return '';
        }
        
        const content = element.innerHTML.trim();
        console.log('getEditorContent returning:', content);
        return content;
    } catch (error) {
        console.error('getEditorContent error:', error);
        return '';
    }
};

// Enhanced setEditorContent with debugging
window.setEditorContent = (element, content) => {
    console.log('setEditorContent called with element:', element, 'content:', content);
    try {
        if (!element) {
            console.warn('setEditorContent: Element is null or undefined');
            return;
        }
        
        element.innerHTML = content || '';
        console.log('setEditorContent: Content set successfully');
    } catch (error) {
        console.error('setEditorContent error:', error);
    }
};

// Enhanced formatText with debugging
window.formatText = (command, value = null) => {
    console.log('formatText called with command:', command, 'value:', value);
    try {
        // Focus the editor first
        const editor = document.querySelector('[data-editor="true"]');
        if (editor) {
            editor.focus();
            console.log('Editor focused for formatting');
        }
        
        // Check if the command is supported
        if (document.queryCommandSupported && !document.queryCommandSupported(command)) {
            console.warn('formatText: Command not supported:', command);
            return;
        }
        
        // Execute the command
        const success = document.execCommand(command, false, value);
        console.log('formatText result:', success ? 'success' : 'failed');
        
        if (!success) {
            console.warn('formatText: Command execution failed for:', command);
        }
    } catch (error) {
        console.error('formatText error:', error);
    }
};

// Initialize editor when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    console.log('DOM loaded - Note editor JavaScript ready');
    
    // Auto-initialize any existing editors
    const editors = document.querySelectorAll('[data-editor="true"]');
    editors.forEach(editor => {
        console.log('Auto-initializing editor:', editor);
        window.initializeEditor(editor);
    });
});

// Enhanced initializeEditor with debugging and error handling
window.initializeEditor = (element) => {
    console.log('initializeEditor called with element:', element);
    
    try {
        if (!element) {
            console.warn('initializeEditor: Element is null or undefined');
            return false;
        }
        
        // Check if already initialized
        if (element.dataset.initialized === 'true') {
            console.log('initializeEditor: Editor already initialized');
            return true;
        }
        
        // Set up paste handler to prevent rich text pasting
        element.addEventListener('paste', function(e) {
            console.log('Paste event triggered');
            e.preventDefault();
            
            try {
                const clipboardData = e.clipboardData || window.clipboardData;
                const text = clipboardData.getData('text/plain') || clipboardData.getData('text');
                
                if (text) {
                    // Try modern approach first
                    if (document.queryCommandSupported && document.queryCommandSupported('insertText')) {
                        document.execCommand('insertText', false, text);
                    } else {
                        // Fallback for older browsers
                        const selection = window.getSelection();
                        if (selection.rangeCount > 0) {
                            const range = selection.getRangeAt(0);
                            range.deleteContents();
                            range.insertNode(document.createTextNode(text));
                            range.collapse(false);
                            selection.removeAllRanges();
                            selection.addRange(range);
                        }
                    }
                    console.log('Paste handled successfully');
                } else {
                    console.warn('No text content found in paste');
                }
            } catch (error) {
                console.error('Paste handler error:', error);
            }
        });
        
        // Set up keyboard shortcuts
        element.addEventListener('keydown', function(e) {
            if (e.ctrlKey || e.metaKey) {
                let command = null;
                
                switch(e.key.toLowerCase()) {
                    case 'b':
                        command = 'bold';
                        break;
                    case 'i':
                        command = 'italic';
                        break;
                    case 'u':
                        command = 'underline';
                        break;
                }
                
                if (command) {
                    e.preventDefault();
                    console.log('Keyboard shortcut triggered:', command);
                    window.formatText(command);
                }
            }
        });
        
        // Add focus and blur handlers for better UX
        element.addEventListener('focus', function() {
            console.log('Editor focused');
            element.classList.add('editor-focused');
        });
        
        element.addEventListener('blur', function() {
            console.log('Editor blurred');
            element.classList.remove('editor-focused');
        });
        
        // Mark as initialized
        element.dataset.initialized = 'true';
        console.log('Editor initialized successfully');
        return true;
        
    } catch (error) {
        console.error('initializeEditor error:', error);
        return false;
    }
};

// Ensure editor is focused when created
window.focusEditor = (element) => {
    console.log('focusEditor called with element:', element);
    try {
        if (element && typeof element.focus === 'function') {
            element.focus();
            console.log('Editor focused successfully');
            return true;
        } else {
            console.warn('focusEditor: Element is not focusable');
            return false;
        }
    } catch (error) {
        console.error('focusEditor error:', error);
        return false;
    }
};

// Debug function to test editor functionality
window.testEditor = () => {
    console.log('Testing editor functionality...');
    const editor = document.querySelector('[data-editor="true"]');
    if (editor) {
        console.log('Editor found:', editor);
        console.log('Editor content:', window.getEditorContent(editor));
        console.log('Trying to set test content...');
        window.setEditorContent(editor, '<p>Test content from JavaScript</p>');
        console.log('Testing format bold...');
        window.formatText('bold');
    } else {
        console.error('No editor found with data-editor="true"');
    }
};

console.log('Note editor JavaScript file loaded successfully');
