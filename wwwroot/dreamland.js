
// dreamland.js, MIT license
const DLFEATURES = ['css', 'jsxLiterals', 'usestring', 'stores']; const DLVERSION = '0.0.20';
(function (exports) {

    class DreamlandError extends Error {
        constructor(message) {
            super('[dreamland-js/dev] ' + message);
            this.name = 'DreamlandDevError';
        }
    }

    function log(message) {
        console.log('[dreamland-js/dev] ' + message);
    }

    function panic(message) {
        throw new DreamlandError('fatal: ' + message)
    }

    function assert(condition, message) {
        if (!condition) {
            panic(message);
        }
    }

    // We add some extra properties into various objects throughout, better to use symbols and not interfere. this is just a tiny optimization
    const [USE_COMPUTED, TARGET, PROXY, STEPS, LISTENERS, IF, STATEHOOK] =
        Array.from(Array(7), Symbol);

    const cssBoundary = 'dlcomponent';

    const cssmap = {};

    /* POLYFILL.SCOPE.START */
    let scopeSupported;
    function checkScopeSupported() {
        if (scopeSupported) return true
        const style = document.createElement('style');
        style.textContent = '@scope (.test) { :scope { color: red } }';
        document.head.appendChild(style);

        const testElement = document.createElement('div');
        testElement.className = 'test';
        document.body.appendChild(testElement);

        const computedColor = getComputedStyle(testElement).color;
        document.head.removeChild(style);
        document.body.removeChild(testElement);

        scopeSupported = computedColor == 'rgb(255, 0, 0)';
        return scopeSupported
    }
    const depth = 50;
    // polyfills @scope for firefox and older browsers, using a :not selector recursively increasing in depth
    // depth 50 means that after 50 layers of nesting, switching between an unrelated component and the target component, it will eventually stop applying styles (or let them leak into children)
    // this is slow. please ask mozilla to implement @scope
    function polyfill_scope(target) {
        let boundary = `:not(${target}).${cssBoundary}`;
        let g = (str, i) =>
            `${str} *${i > depth ? '' : `:not(${g(str + ' ' + (i % 2 == 0 ? target : boundary), i + 1)})`}`;
        return `:not(${g(boundary, 0)})`
    }
    /* POLYFILL.SCOPE.END */

    function genuid() {
        return `${Array(4)
        .fill(0)
        .map(() => {
            return Math.floor(Math.random() * 36).toString(36)
        })
        .join('')}`
    }

    const csstag = (scoped) =>
        function css(strings, ...values) {
            let str = '';
            for (let f of strings) {
                str += f + (values.shift() || '');
            }

            return genCss('dl' + genuid(), str, scoped)
        };

    const css = csstag(false);
    const scope = csstag(true);

    function genCss(uid, str, scoped) {
        let cached = cssmap[str];
        if (cached) return cached

        cssmap[str] = uid;

        const styleElement = document.createElement('style');
        document.head.appendChild(styleElement);

        let newstr = '';
        let selfstr = '';

        str += '\n';
        for (;;) {
            let [first, ...rest] = str.split('\n');
            if (first.trim().endsWith('{')) break

            selfstr += first + '\n';
            str = rest.join('\n');
            if (!str) break
        }

        styleElement.textContent = str;
        if (scoped) {
            /* POLYFILL.SCOPE.START */
            if (!checkScopeSupported()) {
                let scoped = polyfill_scope(`.${uid}`);
                for (const rule of styleElement.sheet.cssRules) {
                    if (rule.selectorText?.startsWith(':'))
                        rule.selectorText = `.${uid}${rule.selectorText}`;
                    else rule.selectorText = `.${uid} ${rule.selectorText}${scoped}`;
                    newstr += rule.cssText + '\n';
                }

                styleElement.textContent = `.${uid} {${selfstr}}` + '\n' + newstr;
                return uid
            }
            /* POLYFILL.SCOPE.END */

            let extstr = '';
            for (const rule of styleElement.sheet.cssRules) {
                if (!rule.selectorText) {
                    extstr += rule.cssText;
                } else if (rule.selectorText?.startsWith(':')) {
                    rule.selectorText = `.${uid}${rule.selectorText}`;
                    extstr += rule.cssText;
                } else {
                    newstr += rule.cssText;
                }
            }

            styleElement.textContent = `.${uid} {${selfstr}} @scope (.${uid}) to (:not(.${uid}).${cssBoundary} *) { ${newstr} } ${extstr}`;
        } else {
            for (const rule of styleElement.sheet.cssRules) {
                rule.selectorText = `.${uid} ${rule.selectorText}`;
                newstr += rule.cssText;
            }

            styleElement.textContent = `.${uid} {${selfstr}}` + '\n' + newstr;
        }

        return uid
    }

    /* FEATURE.CSS.END */

    // saves a few characters, since document will never change
    let doc = document;

    const Fragment = Symbol();

    // whether to return the true value from a stateful object or a "trap" containing the pointer
    let __use_trap = false;

    // Say you have some code like
    //// let state = $state({
    ////    a: $state({
    ////      b: 1
    ////    })
    //// })
    //// let elm = <p>{window.use(state.a.b)}</p>
    //
    // According to the standard, the order of events is as follows:
    // - the getter for window.use gets called, setting __use_trap true
    // - the proxy for state.a is triggered and instead of returning the normal value it returns the trap
    // - the trap proxy is triggered, storing ["a", "b"] as the order of events
    // - the function that the getter of `use` returns is called, setting __use_trap to false and restoring order
    // - the JSX factory h() is now passed the trap, which essentially contains a set of pointers pointing to the theoretical value of b
    // - with the setter on the stateful proxy, we can listen to any change in any of the nested layers and call whatever listeners registered
    // - the result is full intuitive reactivity with minimal overhead
    Object.defineProperty(window, 'use', {
        get: () => {
            __use_trap = true;
            return (ptr, transform, ...rest) => {
                /* FEATURE.USESTRING.START */
                if (ptr instanceof Array) return usestr(ptr, transform, ...rest)
                /* FEATURE.USESTRING.END */
                assert(
                    isDLPtrInternal(ptr) || isDLPtr(ptr),
                    'a value was passed into use() that was not part of a stateful context'
                );
                __use_trap = false;

                let newp = {
                    get value() {
                        return resolve(newp)
                    },
                };

                if (isDLPtr(ptr)) {
                    let cloned = [...ptr[USE_COMPUTED]];
                    if (transform) {
                        cloned.push(transform);
                    }

                    newp[PROXY] = ptr[PROXY];
                    newp[USE_COMPUTED] = cloned;
                } else {
                    newp[PROXY] = ptr;
                    newp[USE_COMPUTED] = transform ? [transform] : [];
                }

                return newp
            }
        },
    });

    /* FEATURE.USESTRING.START */
    const usestr = (strings, ...values) => {
        __use_trap = false;

        let state = $state({});
        const flattened_template = [];
        for (const i in strings) {
            flattened_template.push(strings[i]);
            if (values[i]) {
                let prop = values[i];

                if (isDLPtrInternal(prop)) prop = use(prop);

                if (isDLPtr(prop)) {
                    const current_i = flattened_template.length;
                    let oldparsed;
                    handle(use(prop), (val) => {
                        flattened_template[current_i] = String(val);
                        let parsed = flattened_template.join('');
                        if (parsed != oldparsed) state.string = parsed;
                        oldparsed = parsed;
                    });
                } else {
                    flattened_template.push(String(prop));
                }
            }
        }

        state.string = flattened_template.join('');

        return use(state.string)
    };
    /* FEATURE.USESTRING.END */

    let TRAPS = new Map();
    // This wraps the target in a proxy, doing 2 things:
    // - whenever a property is accessed, return a "trap" that catches and records accessors
    // - whenever a property is set, notify the subscribed listeners
    // This is what makes our "pass-by-reference" magic work
    function $state(target) {
        assert(isobj(target), '$state() requires an object');
        target[LISTENERS] = [];
        target[TARGET] = target;
        let TOPRIMITIVE = Symbol.toPrimitive;

        let proxy = new Proxy(target, {
            get(target, property, proxy) {
                if (__use_trap) {
                    let sym = Symbol();
                    let trap = new Proxy(
                        {
                            [TARGET]: target,
                            [PROXY]: proxy,
                            [STEPS]: [property],
                            [TOPRIMITIVE]: () => sym,
                        },
                        {
                            get(target, property) {
                                if (
                                    [
                                        TARGET,
                                        PROXY,
                                        STEPS,
                                        USE_COMPUTED,
                                        TOPRIMITIVE,
                                    ].includes(property)
                                )
                                    return target[property]
                                property = TRAPS.get(property) || property;
                                target[STEPS].push(property);
                                return trap
                            },
                        }
                    );
                    TRAPS.set(sym, trap);

                    return trap
                }
                return Reflect.get(target, property, proxy)
            },
            set(target, property, val) {
                let trap = Reflect.set(target, property, val);
                for (let listener of target[LISTENERS]) {
                    listener(target, property, val);
                }

                /* FEATURE.STORES.START */
                if (target[STATEHOOK])
                    target[STATEHOOK](target, property, target[property]);
                /* FEATURE.STORES.END */

                return trap
            },
        });

        return proxy
    }

    let isobj = (o) => o instanceof Object;
    let isfn = (o) => typeof o === 'function';

    function isStateful(obj) {
        return isobj(obj) && LISTENERS in obj
    }

    function isDLPtrInternal(arr) {
        return isobj(arr) && STEPS in arr
    }

    function isDLPtr(arr) {
        return isobj(arr) && USE_COMPUTED in arr
    }

    function $if(condition, then, otherwise) {
        otherwise ??= doc.createTextNode('');
        if (!isDLPtr(condition)) return condition ? then : otherwise

        return { [IF]: condition, then, otherwise }
    }

    function resolve(exptr) {
        let proxy = exptr[PROXY];
        let steps = proxy[STEPS];
        let computed = exptr[USE_COMPUTED];

        let val = proxy[TARGET];
        for (let step of steps) {
            val = val[step];
            if (!isobj(val)) break
        }

        for (let transform of computed) {
            val = transform(val);
        }

        return val
    }

    // This lets you subscribe to a stateful object
    function handle(exptr, callback) {
        assert(isDLPtr(exptr), 'handle() requires a stateful object');
        assert(isfn(callback), 'handle() requires a callback function');

        let ptr = exptr[PROXY],
            computed = exptr[USE_COMPUTED],
            step,
            resolvedSteps = [];

        function update() {
            let val = ptr[TARGET];
            for (step of resolvedSteps) {
                val = val[step];
                if (!isobj(val)) break
            }

            for (let transform of computed) {
                val = transform(val);
            }
            callback(val);
        }

        // inject ourselves into nested objects
        let curry = (target, i) =>
            function subscription(tgt, prop, val) {
                if (prop === resolvedSteps[i] && target === tgt) {
                    update();

                    if (isobj(val)) {
                        let v = val[LISTENERS];
                        if (v && !v.includes(subscription)) {
                            v.push(curry(val[TARGET], i + 1));
                        }
                    }
                }
            };

        // imagine we have a `use(state.a[state.b])`
        // simply recursively resolve any of the intermediate steps until we get to the final value
        // this will "misfire" occassionaly with a scenario like state.a[state.b][state.c] and call the listener more than needed
        // it is up to the caller to not implode
        for (let i in ptr[STEPS]) {
            let step = ptr[STEPS][i];
            if (isobj(step) && step[TARGET]) {
                handle(step, (val) => {
                    resolvedSteps[i] = val;
                    update();
                });
                continue
            }
            resolvedSteps[i] = step;
        }

        let sub = curry(ptr[TARGET], 0);
        ptr[TARGET][LISTENERS].push(sub);

        sub(ptr[TARGET], resolvedSteps[0], ptr[TARGET][resolvedSteps[0]]);
    }

    function JSXAddFixedWrapper(ptr, cb, $if) {
        let before, appended, first, flag;
        handle(ptr, (val) => {
            first = appended?.[0];
            if (first) before = first.previousSibling || (flag = first.parentNode);
            if (appended) appended.forEach((a) => a.remove());

            appended = JSXAddChild(
                $if ? (val ? $if.then : $if.otherwise) : val,
                (el) => {
                    if (before) {
                        if (flag) {
                            before.prepend(el);
                            flag = null;
                        } else before.after(el);
                        before = el;
                    } else cb(el);
                }
            );
        });
    }

    // returns a function that sets a reference
    // the currying is a small optimization
    let curryset = (ptr) => (val) => {
        console.log(ptr);
        let next = ptr[PROXY];
        let steps = ptr[STEPS];
        let i = 0;
        for (; i < steps.length - 1; i++) {
            next = next[steps[i]];
            if (!isobj(next)) return
        }
        next[steps[i]] = val;
    };

    // Actual JSX factory. Responsible for creating the HTML elements and all of the *reactive* syntactic sugar
    function h(type, props, ...children) {
        if (type == Fragment) return children
        if (typeof type == 'function') {
            // functional components. create the stateful object
            let newthis = $state(Object.create(type.prototype));

            for (let name in props) {
                let ptr = props[name];
                if (name.startsWith('bind:')) {
                    assert(
                        isDLPtr(ptr),
                        'bind: requires a reference pointer from use'
                    );

                    let set = curryset(ptr[PROXY]);
                    let propname = name.substring(5);
                    if (propname == 'this') {
                        set(newthis);
                    } else {
                        // component two way data binding!! (exact same behavior as svelte:bind)
                        let isRecursive = false;

                        handle(ptr, (value) => {
                            if (isRecursive) {
                                isRecursive = false;
                                return
                            }
                            isRecursive = true;
                            newthis[propname] = value;
                        });
                        handle(use(newthis[propname]), (value) => {
                            if (isRecursive) {
                                isRecursive = false;
                                return
                            }
                            isRecursive = true;
                            set(value);
                        });
                    }
                    delete props[name];
                }
            }
            Object.assign(newthis, props);

            newthis.children = [];
            for (let child of children) {
                JSXAddChild(child, newthis.children.push.bind(newthis.children));
            }

            let elm = type.apply(newthis);
            assert(
                !(elm instanceof Array),
                'Functional component cannot return a Fragment'
            );
            assert(elm instanceof Node, 'Functional component must return a Node');
            assert(
                !('$' in elm),
                'Functional component cannot have another functional component at root level'
            ); // reasoning: it would overwrite data-component and make a mess of the css
            elm.$ = newthis;
            newthis.root = elm;
            /* FEATURE.CSS.START */
            let cl = elm.classList;
            let css = newthis.css;
            let sanitizedName = type.name.replaceAll('$', '-');
            if (css) {
                cl.add(genCss(`${sanitizedName}-${genuid()}`, css, true));
            }
            cl.add(cssBoundary);
            /* FEATURE.CSS.END */
            elm.setAttribute('data-component', type.name);
            if (typeof newthis.mount === 'function') newthis.mount();
            return elm
        }

        let xmlns = props?.xmlns;
        let elm = xmlns ? doc.createElementNS(xmlns, type) : doc.createElement(type);

        for (let child of children) {
            let bappend = elm.append.bind(elm);
            JSXAddChild(child, bappend);
        }

        if (!props) return elm

        let useProp = (name, callback) => {
            if (!(name in props)) return
            let prop = props[name];
            callback(prop);
            delete props[name];
        };

        for (let name in props) {
            let ptr = props[name];
            if (name.startsWith('bind:')) {
                assert(isDLPtr(ptr), 'bind: requires a reference pointer from use');
                let propname = name.substring(5);

                // create the function to set the value of the pointer
                let set = curryset(ptr[PROXY]);
                if (propname == 'this') {
                    set(elm);
                } else if (propname == 'value') {
                    handle(ptr, (value) => (elm.value = value));
                    elm.addEventListener('change', () => set(elm.value));
                } else if (propname == 'checked') {
                    handle(ptr, (value) => (elm.checked = value));
                    elm.addEventListener('click', () => set(elm.checked));
                }
                delete props[name];
            }
            if (name == 'style' && isobj(ptr) && !isDLPtr(ptr)) {
                for (let key in ptr) {
                    let prop = ptr[key];
                    if (isDLPtr(prop)) {
                        handle(prop, (value) => (elm.style[key] = value));
                    } else {
                        elm.style[key] = prop;
                    }
                }
                delete props[name];
            }
        }

        useProp('class', (classlist) => {
            assert(
                typeof classlist === 'string' || classlist instanceof Array,
                'class must be a string or array'
            );
            if (typeof classlist === 'string') {
                elm.setAttribute('class', classlist);
                return
            }

            if (isDLPtr(classlist)) {
                handle(classlist, (classname) =>
                    elm.setAttribute('class', classname)
                );
                return
            }

            for (let name of classlist) {
                if (isDLPtr(name)) {
                    let oldvalue = null;
                    handle(name, (value) => {
                        if (typeof oldvalue === 'string') {
                            elm.classList.remove(oldvalue);
                        }
                        elm.classList.add(value);
                        oldvalue = value;
                    });
                } else {
                    elm.classList.add(name);
                }
            }
        });

        // apply the non-reactive properties
        for (let name in props) {
            let prop = props[name];
            if (isDLPtr(prop)) {
                handle(prop, (val) => {
                    JSXAddAttributes(elm, name, val);
                });
            } else {
                JSXAddAttributes(elm, name, prop);
            }
        }

        // hack to fix svgs
        if (xmlns) elm.innerHTML = elm.innerHTML;

        return elm
    }

    // glue for nested children
    function JSXAddChild(child, cb) {
        let childchild, elms, node;
        if (isDLPtr(child)) {
            JSXAddFixedWrapper(child, cb);
        } else if (isobj(child) && IF in child) {
            JSXAddFixedWrapper(child[IF], cb, child);
        } else if (child instanceof Node) {
            cb(child);
            return [child]
        } else if (child instanceof Array) {
            elms = [];

            for (childchild of child) {
                elms = elms.concat(JSXAddChild(childchild, cb));
            }
            if (!elms[0]) elms = JSXAddChild('', cb);
            return elms
        } else {
            // this is what makes it so that {null} won't render. the empty string would seem odd coming from other frameworks but it is for the best
            if (child === null || child === undefined) child = '';

            node = doc.createTextNode(child);
            cb(node);
            return [node]
        }
    }

    // Where properties are assigned to elements, and where the *non-reactive* syntax sugar goes
    function JSXAddAttributes(elm, name, prop) {
        if (name.startsWith('on:')) {
            assert(typeof prop === 'function', 'on: requires a function');
            let names = name.substring(3);
            for (let name of names.split('$')) {
                elm.addEventListener(name, (...args) => {
                    self.$el = elm;
                    prop(...args);
                });
            }
            return
        }

        elm.setAttribute(name, prop);
    }

    function html(strings, ...values) {
        // normalize the strings array, it would otherwise give us an object
        strings = [...strings];
        let flattened = '';
        let markers = {};
        for (let i = 0; i < strings.length; i++) {
            let string = strings[i];
            let value = values[i];

            // since self closing tags don't exist in regular html, look for the pattern <tag /> enclosing a function, and replace it with `<tag`
            let match =
                values[i] instanceof Function && /^ *\/>/.exec(strings[i + 1]);
            if (/< *$/.test(string) && match) {
                strings[i + 1] = strings[i + 1].substr(
                    match.index + match[0].length
                );
            }

            flattened += string;
            if (i < values.length) {
                let dupe = Object.values(markers).findIndex((v) => v === value);
                let marker;
                if (dupe !== -1) {
                    marker = Object.keys(markers)[dupe];
                } else {
                    marker = 'h' + genuid();
                    markers[marker] = value;
                }

                flattened += marker;

                // close the self closing tag
                if (match) {
                    flattened += `></${marker}>`;
                }
            }
        }
        let dom = new DOMParser().parseFromString(flattened, 'text/html');
        assert(
            dom.body.children.length == 1,
            'html builder needs exactly one child'
        );

        function wraph(elm) {
            let nodename = elm.nodeName.toLowerCase();
            if (nodename === '#text') return elm.textContent
            if (nodename in markers) nodename = markers[nodename];

            let children = [...elm.childNodes].map(wraph);
            for (let i = 0; i < children.length; i++) {
                let text = children[i];
                if (typeof text !== 'string') continue
                for (const [marker, value] of Object.entries(markers)) {
                    if (!text) break
                    if (!text.includes(marker)) continue
                    let before
                    ;[before, text] = text.split(marker);
                    children = [
                        ...children.slice(0, i),
                        before,
                        value,
                        text,
                        ...children.slice(i + 1),
                    ];
                    i += 2;
                }
            }

            let attributes = {};

            if (!elm.attributes) return elm // passthrough comments
            for (const attr of [...elm.attributes]) {
                let val = attr.nodeValue;
                if (val in markers) val = markers[val];
                attributes[attr.name] = val;
            }

            return h(nodename, attributes, children)
        }

        return wraph(dom.body.children[0])
    }

    function $store(target, { ident, backing, autosave }) {
        let read, write;
        if (typeof backing === 'string') {
            switch (backing) {
                case 'localstorage':
                    read = () => localStorage.getItem(ident);
                    write = (ident, data) => {
                        localStorage.setItem(ident, data);
                    };
                    break
                default:
                    assert('Unknown store type: ' + backing);
            }
        } else {
    ({ read, write } = backing);
        }

        let save = () => {
            console.info('[dreamland.js]: saving ' + ident);

            // stack gets filled with "pointers" representing unique objects
            // this is to avoid circular references

            let serstack = {};
            let vpointercount = 0;

            let ser = (tgt) => {
                let obj = {
                    stateful: isStateful(tgt),
                    values: {},
                };
                let i = vpointercount++;
                serstack[i] = obj;

                for (let key in tgt) {
                    let value = tgt[key];

                    if (isDLPtr(value)) continue // i don"t think we should be serializing pointers?
                    switch (typeof value) {
                        case 'string':
                        case 'number':
                        case 'boolean':
                        case 'undefined':
                            obj.values[key] = JSON.stringify(value);
                            break

                        case 'object':
                            if (value instanceof Array) {
                                obj.values[key] = value.map((v) => {
                                    if (typeof v === 'object') {
                                        return ser(v)
                                    } else {
                                        return JSON.stringify(v)
                                    }
                                });
                                break
                            } else {
                                assert(
                                    value.__proto__ === Object.prototype,
                                    'Only plain objects are supported'
                                );
                                obj.values[key] = ser(value);
                            }
                            break

                        case 'symbol':
                        case 'function':
                        case 'bigint':
                            assert('Unsupported type: ' + typeof value);
                            break
                    }
                }

                return i
            };
            ser(target);

            let string = JSON.stringify(serstack);
            write(ident, string);
        };

        let autohook = (target, prop, value) => {
            if (isStateful(value)) value[TARGET][STATEHOOK] = autohook;
            save();
        };

        let destack = JSON.parse(read(ident));
        if (destack) {
            let objcache = {};

            let de = (i) => {
                if (objcache[i]) return objcache[i]
                let obj = destack[i];
                let tgt = {};
                for (let key in obj.values) {
                    let value = obj.values[key];
                    if (typeof value === 'string') {
                        tgt[key] = JSON.parse(value);
                    } else {
                        if (value instanceof Array) {
                            tgt[key] = value.map((v) => {
                                if (typeof v === 'string') {
                                    return JSON.parse(v)
                                } else {
                                    return de(v)
                                }
                            });
                        } else {
                            tgt[key] = de(value);
                        }
                    }
                }
                if (obj.stateful && autosave == 'auto') tgt[STATEHOOK] = autohook;
                let newobj = obj.stateful ? $state(tgt) : tgt;
                objcache[i] = newobj;
                return newobj
            };

            target = de(0);
        }
        switch (autosave) {
            case 'beforeunload':
                addEventListener('beforeunload', save);
                break
            case 'manual':
                break
            case 'auto':
                target[STATEHOOK] = autohook;
                break
            default:
                assert('Unknown autosave type: ' + autosave);
        }

        return $state(target)
    }

    log('Version: ' + DLVERSION);
    console.warn(
        'This is a DEVELOPER build of dreamland.js. It is not suitable for production use.'
    );
    console.info('Enabled features:', DLFEATURES.join(', '));
    /* DEV.END */

    exports.$if = $if;
    exports.$state = $state;
    exports.$store = $store;
    exports.Fragment = Fragment;
    exports.css = css;
    exports.h = h;
    exports.handle = handle;
    exports.html = html;
    exports.isDLPtr = isDLPtr;
    exports.isStateful = isStateful;
    exports.scope = scope;
    exports.stateful = $state;

})(window)
//# sourceMappingURL=dev.js.map
