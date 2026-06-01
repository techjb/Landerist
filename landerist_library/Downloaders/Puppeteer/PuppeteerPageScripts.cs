namespace landerist_library.Downloaders.Puppeteer
{
    internal static class PuppeteerPageScripts
    {
        public const string RemoveCookies =
            @"document.querySelectorAll('[class*=""cookie"" i], [id*=""cookie"" i]').forEach(el => el.remove());";

        public const string DeleteWebdriver= @"() => { delete navigator.__proto__.webdriver; }";

        public const string RemoveInvisibleElements = @"
            () => {
                const isVisible = (elem) => {
                    if (!(elem instanceof Element)) return false;
                    const style = window.getComputedStyle(elem);
                    if (style.display === 'none' || style.visibility === 'hidden' || style.opacity === '0') {
                        return false;
                    }
                    return true;
                };

                const removeInvisibleElements = (root) => {
                    if (!(root instanceof Node)) {
                        return;
                    }

                    const walker = document.createTreeWalker(root, NodeFilter.SHOW_ELEMENT, null, false);
                    const toRemove = [];
                    while (walker.nextNode()) {
                        const node = walker.currentNode;
                        if (!isVisible(node)) {
                            toRemove.push(node);
                        }
                    }
                    for (const node of toRemove) {
                        node.remove();
                    }
                };

                const root = document.body ?? document.documentElement;
                removeInvisibleElements(root);
            }
        ";
    }
}
