﻿'use strict';

(function() {

    const URL_VALUES = 'api/Values';
    const URL_PROTECTED_VALUES = 'api/ProtectedValues';
    const JWT_ADMIN_FALSE = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJkY3RoYW5nQGdtYWlsLmNvbSIsIm5hbWUiOiJUaGFuZyBEdW9uZyIsImFkbWluIjpmYWxzZX0.K3cIOfqduZcb1NmKXrOpxOy1z58MmahFaPgOkd2Swxw';
    const JWT_ADMIN_TRUE = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJkY3RoYW5nQGdtYWlsLmNvbSIsIm5hbWUiOiJUaGFuZyBEdW9uZyIsImFkbWluIjp0cnVlfQ.sOvVeT89cDO00PfB_U5RZHJQ8Lq3SI56JUs8PwmgGz8';

    const GET = Symbol('HTTP Verb');
    const POST = Symbol('HTTP Verb');
    const API_VALUES = Symbol('API Resource');
    const API_PROTECTED_VALUES = Symbol('API Resource');
    const USER_TOKEN = Symbol('Token Type');
    const ADMIN_TOKEN = Symbol('Token Type');

    // an ever-increasing count to make POST data unique for api/ProtectedValues
    let count = 9999;

    // polyfill + short-hand method for console.log
    const log = console.log.bind(console);

    class FetchParameterFactory {

        /**
        * @param {string} data - the data to be embeded in the body of the POST request
        */
        static createUrlSearchParam(text) {
            const dataType = typeof text;
            if (dataType !== 'string') {
                throw new Error(`Invalid argument type. A string is expected but the actual type is: ${dataType}`);
            }

            const param = new URLSearchParams();
            // The client needs to send the value with the following format:
            //    =value
            // according to http://www.asp.net/web-api/overview/advanced/sending-html-form-data-part-1
            param.append('', text);
            return param;
        }

        static create(configurations) {
            let url = '';
            let options = {};

            const routes = [{
                symbol: API_VALUES,
                url: URL_VALUES
            }, {
                symbol: API_PROTECTED_VALUES,
                url: URL_PROTECTED_VALUES
            }];

            const rules = [{
                symbol: GET,
                options: {
                    method: 'GET'
                }
            }, {
                symbol: POST,
                options: {
                    method: 'POST',
                    body: FetchParameterFactory.createUrlSearchParam(`gold ${count++}`)
                }
            }, {
                symbol: USER_TOKEN,
                options: {
                    headers: new Headers({
                        'Authorization': `Bearer ${JWT_ADMIN_FALSE}`
                    })
                }
            }, {
                symbol: ADMIN_TOKEN,
                options: {
                    headers: new Headers({
                        'Authorization': `Bearer ${JWT_ADMIN_TRUE}`
                    })
                }
            }];

            configurations.forEach(config => {
                const route = routes.find(first => first.symbol === config);
                if (route) {
                    url = route.url;
                    log(`Route found. Url: ${url}`);
                }

                const rule = rules.find(first => first.symbol === config);
                if (rule) {
                    options = Object.assign(options, rule.options);
                    log(`Options found. Options: ${JSON.stringify(options)}`);
                }
            });

            return { url, options };
        }
    }

    class PageView {

        /**
         * @param {Array} configurations - an array of symbols, each symbol is an instruction to build AJAX requests
         * @return {function} - an event listener function
         */
        static createEventListener(configurations) {

            const { url, options } = FetchParameterFactory.create(configurations);

            if (!url || !Object.keys(options).length) {
                throw new Error(`Cannot create event listener. Url: ${url}. Options: ${options}`);
            }

            return function makeAjaxRequest() {
                fetch(url, options).then(response => response.json())
                  .then(json => log(`Received json: ${json}`))
                  .catch(error => log(`Ajax request failed. Error: ${error}`));
            };
        }

        constructor() {
            this.buttons = [];
        }

        addButton({ id, onClick }) {
          const btn = document.getElementById(id);
            btn.addEventListener('click', onClick);
            this.buttons.push(btn);
        }

        addButtons(buttons) {
            buttons.forEach(btn => this.addButton(btn));
        }
    }

    class PageController {
        constructor(buttonConfigurations) {
            const view = new PageView();
            view.addButtons(Object.keys(buttonConfigurations).map(buttonId => {
                const listener = PageView.createEventListener(buttonConfigurations[buttonId]);
                return {
                    id: buttonId,
                    onClick: listener
                };
            }));
            this.view = view;
        }
    }

    const pageController = new PageController({
        btnAnonymousGet: [GET, API_VALUES],
        btnUnauthenticatedGet: [GET, API_PROTECTED_VALUES],
        btnAuthenticatedGet: [GET, USER_TOKEN, API_PROTECTED_VALUES],
        btnUnauthorizedPOST: [POST, USER_TOKEN, API_PROTECTED_VALUES],
        btnAuthorizedPOST: [POST, ADMIN_TOKEN, API_PROTECTED_VALUES],
    });
})();