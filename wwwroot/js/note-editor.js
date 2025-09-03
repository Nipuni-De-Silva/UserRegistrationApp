// Note Editor JavaScript Functions
window.getEditorContent = (element) => {
    if (!element) return '';
    return element.innerHTML || '';
};

window.setEditorContent = (element, content) => {
    if (element) {
        element.innerHTML = content || '';
    }
};

window.formatText = (command, value = null) => {
    try {
        document.execCommand(command, false, value);
    } catch (error) {
        console.error('Format text error:', error);
    }
};

// Initialize editor when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    // Add any editor initialization here
    console.log('Note editor JavaScript loaded');
});

// Handle editor focus and blur events
window.initializeEditor = (element) => {
    if (!element) return;
    
    element.addEventListener('paste', function(e) {
        e.preventDefault();
        var text = (e.originalEvent || e).clipboardData.getData('text/plain');
        document.execCommand('insertText', false, text);
    });
    
    element.addEventListener('keydown', function(e) {
        // Handle keyboard shortcuts
        if (e.ctrlKey || e.metaKey) {
            switch(e.key) {
                case 'b':
                    e.preventDefault();
                    document.execCommand('bold');
                    break;
                case 'i':
                    e.preventDefault();
                    document.execCommand('italic');
                    break;
                case 'u':
                    e.preventDefault();
                    document.execCommand('underline');
                    break;
            }
        }
    });
};
