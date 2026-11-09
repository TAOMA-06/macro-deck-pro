(function () {
	"use strict";

	// ============================================================
	//  i18n / Internationalization
	// ============================================================
	var i18n = {
		lang: "en",
		strings: {
			en: {
				back: "Back",
				fullscreen: "Fullscreen",
				light: "Light",
				dark: "Dark",
				newConnection: "New connection",
				recentConnections: "Recent connections",
				connect: "Connect",
				ipPlaceholder: "IP address/hostname",
				portPlaceholder: "Port",
				imprint: "Imprint",
				privacy: "Privacy statement",
				terms: "Terms of use",
				webClientLabel: "Web client",
				clientIdLabel: "Client id",
				cookieTitle: "Cookies",
				cookieBody: "We care about privacy! This website uses only technically necessary cookies.",
				cookieLearn: "Learn more about cookies",
				cookieClose: "Close",
				waitingAccept: "Waiting for accepting the connection...",
				reconnecting: "Connection lost. Reconnecting...",
				reconnectFailed: "Unable to reconnect. Reloading...",
				connected: "Connected",
				disconnected: "Disconnected",
				version: "Version"
			},
			zh: {
				back: "返回",
				fullscreen: "全屏",
				light: "浅色",
				dark: "深色",
				newConnection: "新建连接",
				recentConnections: "最近连接",
				connect: "连接",
				ipPlaceholder: "IP地址/主机名",
				portPlaceholder: "端口",
				imprint: "版权声明",
				privacy: "隐私政策",
				terms: "使用条款",
				webClientLabel: "网页客户端",
				clientIdLabel: "客户端 ID",
				cookieTitle: "Cookie 声明",
				cookieBody: "我们重视隐私！本网站仅使用技术必需的 Cookie。",
				cookieLearn: "了解更多关于 Cookie",
				cookieClose: "关闭",
				waitingAccept: "等待连接接受中...",
				reconnecting: "连接已断开，正在重连...",
				reconnectFailed: "无法重连，正在刷新页面...",
				connected: "已连接",
				disconnected: "连接断开",
				version: "版本"
			}
		},
		getString: function (key) {
			return (this.strings[this.lang] && this.strings[this.lang][key]) || this.strings.en[key] || key;
		},
		detect: function () {
			var lang = (navigator.language || navigator.userLanguage || "").toLowerCase();
			this.lang = lang.indexOf("zh") === 0 ? "zh" : "en";
		},
		apply: function () {
			var elements = document.querySelectorAll("[data-i18n]");
			for (var i = 0; i < elements.length; i++) {
				var key = elements[i].getAttribute("data-i18n");
				if (!key) continue;
				var placeholder = elements[i].getAttribute("data-i18n-placeholder");
				if (placeholder !== null) {
					elements[i].setAttribute("placeholder", this.getString(key));
				} else if (elements[i].tagName === "INPUT") {
					// skip value (IP/port should stay user-input)
				} else {
					elements[i].textContent = this.getString(key);
				}
			}
		}
	};
	// detect language on load
	i18n.detect();

	// ============================================================
	//  State
	// ============================================================
	var buttons;
	var websocket;
	var connected = false;
	var recentConnections = [];
	var clientId;
	var rows = 3;
	var columns = 5;
	var buttonSpacing = 5;
	var buttonRadius = 40;
	var buttonSize = 100;
	var buttonBackground = false;
	var icons = [];
	var pressTimer;
	var longPress = false;
	var supportButtonReleaseLongPress = false;
	var buttonsGenerated = false;
	var reconnectAttempts = 0;
	var maxReconnectAttempts = 5;
	var reconnectDelay = 3000;
	var reconnectTimer = null;
	var lastConnectUrl = null;
	var dark = true;

	var apiVersion = 20;
	var version = "2.5.2";

	// ============================================================
	//  Connection status indicator
	// ============================================================
	function setConnectionStatus(statusClass, textKey) {
		var statusEl = document.getElementById("connection-status");
		if (!statusEl) return;
		statusEl.className = "connection-status " + statusClass;
		statusEl.title = i18n.getString(textKey);
		var dot = statusEl.querySelector(".status-dot");
		if (dot) dot.className = "status-dot " + statusClass;
		var text = statusEl.querySelector(".status-text");
		if (text) text.textContent = i18n.getString(textKey);
	}

	function showReconnectingOverlay(show) {
		var overlay = document.getElementById("reconnecting-overlay");
		if (!overlay) return;
		overlay.classList.toggle("d-none", !show);
		if (show) {
			overlay.querySelector("span").textContent =
				reconnectAttempts >= maxReconnectAttempts
					? i18n.getString("reconnectFailed")
					: i18n.getString("reconnecting");
		}
	}

	// ============================================================
	//  Dark mode persistence
	// ============================================================
	function loadDarkPreference() {
		var saved = localStorage.getItem("macrodeck_dark");
		if (saved !== null) {
			dark = saved === "true";
		}
		applyDarkMode();
	}

	function saveDarkPreference() {
		localStorage.setItem("macrodeck_dark", dark ? "true" : "false");
	}

	function applyDarkMode() {
		document.getElementById("body").classList.toggle("bg-dark", dark);
		document.getElementById("btn-dark").textContent = dark ? i18n.getString("light") : i18n.getString("dark");
		var logo = document.getElementById("logo");
		if (logo) {
			logo.setAttribute("src", dark ? "images/logo.png" : "images/logo2.png");
		}
	}

	function toggleDark() {
		dark = !dark;
		applyDarkMode();
		saveDarkPreference();
	}

	// ============================================================
	//  PWA standalone detection
	// ============================================================
	function isPwaStandalone() {
		return (
			window.matchMedia("(display-mode: standalone)").matches ||
			window.navigator.standalone === true
		);
	}

	function updatePwaUi() {
		var standalone = isPwaStandalone();
		var btnBack = document.getElementById("btn-back");
		var btnFullscreen = document.getElementById("btn-fullscreen");
		if (btnBack) btnBack.classList.toggle("d-none", standalone || !connected);
		if (btnFullscreen) btnFullscreen.classList.toggle("d-none", standalone);
	}

	// ============================================================
	//  Disconnect / Back
	// ============================================================
	function back() {
		disconnect();
		window.location.reload(false);
	}

	function disconnect() {
		if (reconnectTimer) {
			clearTimeout(reconnectTimer);
			reconnectTimer = null;
		}
		lastConnectUrl = null;
		reconnectAttempts = 0;
		if (websocket != null) {
			try { websocket.close(); } catch (e) { }
			websocket = null;
		}
		connected = false;
		setConnectionStatus("disconnected", "disconnected");
	}

	// ============================================================
	//  Resize handlers
	// ============================================================
	$(window).resize(function () {
		autoSize();
		updatePwaUi();
	});

	$(window).on("orientationchange", function () {
		autoSize();
	});

	window.addEventListener("resize", function () {
		if (!document.fullscreenElement) {
			if (document.getElementById("btn-back") && !isPwaStandalone()) {
				document.getElementById("btn-back").classList.toggle("d-none", !connected);
			}
			if (document.getElementById("btn-fullscreen")) {
				document.getElementById("btn-fullscreen").classList.toggle("d-none", isPwaStandalone());
			}
			if (document.getElementById("btn-dark")) {
				document.getElementById("btn-dark").classList.toggle("d-none", false);
			}
		} else {
			if (document.getElementById("btn-back")) {
				document.getElementById("btn-back").classList.toggle("d-none", true);
			}
			if (document.getElementById("btn-fullscreen")) {
				document.getElementById("btn-fullscreen").classList.toggle("d-none", true);
			}
			if (document.getElementById("btn-dark")) {
				document.getElementById("btn-dark").classList.toggle("d-none", true);
			}
		}
		autoSize();
	});

	// ============================================================
	//  Fullscreen
	// ============================================================
	function openFullscreen() {
		var elem = document.documentElement;
		if (elem.requestFullscreen) {
			elem.requestFullscreen();
		} else if (elem.webkitRequestFullscreen) {
			elem.webkitRequestFullscreen();
		} else if (elem.msRequestFullscreen) {
			elem.msRequestFullscreen();
		}
	}

	// ============================================================
	//  connect()
	// ============================================================
	function connect(url) {
		if (connected) return;
		if (reconnectTimer) {
			clearTimeout(reconnectTimer);
			reconnectTimer = null;
		}

		lastConnectUrl = url;
		document.getElementById("button-connect-spinner").classList.toggle("d-none", false);
		setConnectionStatus("reconnecting", "reconnecting");

		if (websocket != null) {
			try { websocket.close(); } catch (e) { }
			websocket = null;
		}

		websocket = new WebSocket(url);

		websocket.onopen = function () {
			connected = true;
			reconnectAttempts = 0;
			setConnectionStatus("connected", "connected");
			showReconnectingOverlay(false);

			document.getElementById("button-container").innerHTML = "";
			document.getElementById("connect-container").innerHTML =
				'<div class="d-flex align-items-center justify-content-center" style="height: 500px;">' +
				'<h1>' + i18n.getString("waitingAccept") + ' <span class="spinner-border spinner-border-lg" role="status" aria-hidden="true"></span></h1>' +
				'</div>';

			var jsonObj = {
				"Method": JsonMethod.CONNECTED,
				"Client-Id": clientId,
				"API": apiVersion,
				"Device-Type": "Web"
			};
			doSend(JSON.stringify(jsonObj));
		};

		websocket.onclose = function () {
			connected = false;
			setConnectionStatus("disconnected", "disconnected");

			if (reconnectAttempts < maxReconnectAttempts && lastConnectUrl) {
				reconnectAttempts++;
				showReconnectingOverlay(true);
				reconnectTimer = setTimeout(function () {
					connect(lastConnectUrl);
				}, reconnectDelay);
			} else {
				showReconnectingOverlay(true);
				// show failure message briefly then reload
				setTimeout(function () {
					window.location.reload(false);
				}, 2000);
			}
		};

		websocket.onerror = function () {
			document.getElementById("button-connect-spinner").classList.toggle("d-none", true);
			setConnectionStatus("disconnected", "disconnected");
			try { websocket.close(); } catch (e) { }
			connected = false;
			if (typeof $("#connection-failed-modal").modal === "function") {
				$("#connection-failed-modal").modal("show");
			}
		};

		websocket.onmessage = function (e) {
			try {
				var obj = JSON.parse(e.data);
				switch (obj.Method) {
					case JsonMethod.GET_CONFIG:
						document.getElementById("connect-container").innerHTML = "";
						if (obj.Columns !== columns || obj.Rows !== rows || obj.ButtonBackground != buttonBackground) {
							buttonsGenerated = false;
							columns = obj.Columns;
							rows = obj.Rows;
							buttonBackground = obj.ButtonBackground;
						}
						columns = obj.Columns;
						rows = obj.Rows;
						buttonSpacing = obj.ButtonSpacing;
						buttonRadius = obj.ButtonRadius;
						buttonBackground = obj.ButtonBackground;
						if (obj.SupportButtonReleaseLongPress && obj.SupportButtonReleaseLongPress == true) {
							supportButtonReleaseLongPress = true;
						}

						var jsonObj = { "Method": JsonMethod.GET_BUTTONS };
						doSend(JSON.stringify(jsonObj));

						updatePwaUi();
						if (recentConnections.indexOf(url) === -1) {
							recentConnections.push(url);
							setCookie("recentConnections", JSON.stringify(recentConnections), 365);
						}
						autoSize();
						break;

					case JsonMethod.GET_BUTTONS:
						if (!buttonsGenerated) {
							buttonsGenerated = true;
							generateGrid(columns, rows);
						}
						var actionButtons = document.getElementsByClassName("action-button");
						var labels = document.getElementsByClassName("label");
						for (var i = 0; i < actionButtons.length; i++) {
							actionButtons[i].style.backgroundImage = "";
							actionButtons[i].classList.toggle("btn-secondary", true);
							labels[i].style.backgroundImage = "";
						}

						this.buttons = obj.Buttons;
						for (var i = 0; i < this.buttons.length; i++) {
							var button = document.getElementById(this.buttons[i].Position_Y + "_" + this.buttons[i].Position_X);
							if (button && this.buttons[i]) {
								if (this.buttons[i].Icon) {
									var iconPack;
									var icon;
									if (Array.prototype.find != null) {
										var _buttons = this.buttons;
										iconPack = icons.IconPacks.find(function (e) {
											return e.Name == _buttons[i].Icon.split(".")[0];
										});
										icon = iconPack.Icons.find(function (e) {
											return e.IconId == _buttons[i].Icon.split(".")[1];
										});
									} else {
										for (var j = 0; j < icons.IconPacks.length; j++) {
											if (icons.IconPacks[j].Name == this.buttons[i].Icon.split(".")[0]) {
												iconPack = icons.IconPacks[j];
											}
										}
										for (var j = 0; j < iconPack.Icons.length; j++) {
											if (iconPack.Icons[j].IconId == this.buttons[i].Icon.split(".")[1]) {
												icon = iconPack.Icons[j];
											}
										}
									}
									button.style.backgroundImage = "url(data:image/gif;base64," + icon.IconBase64 + ")";
								} else if (this.buttons[i].IconBase64) {
									button.style.backgroundImage = "url(data:image/gif;base64," + this.buttons[i].IconBase64 + ")";
								}
							}

							var label = document.getElementById("label_" + this.buttons[i].Position_Y + "_" + this.buttons[i].Position_X);
							if (label) {
								if (this.buttons[i].Label && this.buttons[i].Label.LabelBase64) {
									label.style.backgroundImage = "url(data:image/gif;base64," + this.buttons[i].Label.LabelBase64 + ")";
								}
								if (this.buttons[i].LabelBase64) {
									label.style.backgroundImage = "url(data:image/gif;base64," + this.buttons[i].LabelBase64 + ")";
								}
							}

							if (button && this.buttons[i].BackgroundColorHex) {
								button.classList.toggle("btn-secondary", !this.buttons[i].BackgroundColorHex);
								button.style.backgroundColor = this.buttons[i].BackgroundColorHex;
							}
						}
						autoSize();
						break;

					case JsonMethod.UPDATE_BUTTON:
						var button = document.getElementById(obj.Buttons[0].Position_Y + "_" + obj.Buttons[0].Position_X);
						if (button) {
							if (obj.Buttons[0].Icon) {
								var iconPack;
								var icon;
								if (Array.prototype.find != null) {
									iconPack = icons.IconPacks.find(function (e) {
										return e.Name == obj.Buttons[0].Icon.split(".")[0];
									});
									icon = iconPack.Icons.find(function (e) {
										return e.IconId == obj.Buttons[0].Icon.split(".")[1];
									});
								} else {
									for (var j = 0; j < icons.IconPacks.length; j++) {
										if (icons.IconPacks[j].Name == obj.Buttons[0].Icon.split(".")[0]) {
											iconPack = icons.IconPacks[j];
										}
									}
									for (var j = 0; j < iconPack.Icons.length; j++) {
										if (iconPack.Icons[j].IconId == obj.Buttons[0].Icon.split(".")[1]) {
											icon = iconPack.Icons[j];
										}
									}
								}
								button.style.backgroundImage = "url(data:image/gif;base64," + icon.IconBase64 + ")";
							} else if (obj.Buttons[0].IconBase64) {
								button.style.backgroundImage = "url(data:image/gif;base64," + obj.Buttons[0].IconBase64 + ")";
							} else {
								button.style.backgroundImage = "";
							}
							if (button && obj.Buttons[0].BackgroundColorHex) {
								button.classList.toggle("btn-secondary", !obj.Buttons[0].BackgroundColorHex);
								button.style.backgroundColor = obj.Buttons[0].BackgroundColorHex;
							}
						}

						var label = document.getElementById("label_" + obj.Buttons[0].Position_Y + "_" + obj.Buttons[0].Position_X);
						if (label) {
							if (obj.Buttons[0].Label && obj.Buttons[0].Label.LabelBase64) {
								label.style.backgroundImage = "url(data:image/gif;base64," + obj.Buttons[0].Label.LabelBase64 + ")";
							}
							if (obj.Buttons[0].LabelBase64) {
								label.style.backgroundImage = "url(data:image/gif;base64," + obj.Buttons[0].LabelBase64 + ")";
							}
						}
						break;

					case JsonMethod.UPDATE_LABEL:
						var labelEl = document.getElementById("label_" + obj.Buttons[0].Position_Y + "_" + obj.Buttons[0].Position_X);
						if (labelEl) {
							if (obj.Buttons[0].Label && obj.Buttons[0].Label.LabelBase64) {
								labelEl.style.backgroundImage = "url(data:image/gif;base64," + obj.Buttons[0].Label.LabelBase64 + ")";
							}
							if (obj.Buttons[0].LabelBase64) {
								labelEl.style.backgroundImage = "url(data:image/gif;base64," + obj.Buttons[0].LabelBase64 + ")";
							}
						}
						break;

					case JsonMethod.GET_ICONS:
						icons = obj;
						break;

					case JsonMethod.KEEPALIVE:
						// Server heartbeat: no action needed, just confirms connection is alive
						break;
				}
			} catch (err) {
				console.log(err);
				if (document.getElementById("button-connect-spinner")) {
					document.getElementById("button-connect-spinner").classList.toggle("d-none", true);
				}
			}
		};
	}

	// ============================================================
	//  Grid generation
	// ============================================================
	function generateGrid(columns, rows) {
		var buttonContainer = document.getElementById("button-container");
		buttonContainer.innerHTML = "";

		for (var i = 0; i < rows; i++) {
			var row = document.createElement("div");
			row.setAttribute("class", "row");
			row.setAttribute("style", "margin: 0; padding: 0;");
			buttonContainer.appendChild(row);
			for (var j = 0; j < columns; j++) {
				var column = document.createElement("div");
				column.setAttribute("id", "col_" + i + "_" + j);
				column.classList.add("col");
				column.classList.add("blockBox");
				var button = document.createElement("div");
				button.classList.add("action-button");
				button.classList.toggle("btn-secondary", buttonBackground);
				button.setAttribute("id", i + "_" + j);

				$(button).bind("touchstart", function () {
					onTouchStart(this.id);
				});
				$(button).bind("touchend", function () {
					onTouchEnd(this.id);
				});
				$(button).mousedown(function () {
					onMouseDown(this.id);
				});
				$(button).mouseup(function () {
					onMouseUp(this.id);
				});

				var label = document.createElement("div");
				label.setAttribute("id", "label_" + i + "_" + j);
				label.classList.add("label");
				button.appendChild(label);

				var loaderContainer = document.createElement("div");
				loaderContainer.classList.add("loader-container");
				loaderContainer.setAttribute("id", "loader_" + i + "_" + j);
				loaderContainer.classList.toggle("d-none", true);
				button.appendChild(loaderContainer);

				row.appendChild(column);
				column.appendChild(button);
			}
		}
	}

	// ============================================================
	//  autoSize
	// ============================================================
	function autoSize() {
		var divs = document.getElementsByClassName("blockBox");
		var rowsEls = document.getElementsByClassName("row");
		var container = document.getElementsByClassName("button-container")[0];
		var off = 0;
		if (!document.fullscreenElement) {
			var btnFullscreen = document.getElementById("btn-fullscreen");
			off = 30 + (btnFullscreen ? btnFullscreen.offsetHeight * 2 : 66);
		}
		var rowsCount = rowsEls.length;
		var columnsCount = divs.length / rowsCount;
		var width = window.innerWidth;
		var height = window.innerHeight - off;
		var buttonSizeX = width / columnsCount;
		var buttonSizeY = height / rowsCount;
		var btnSize = Math.min(buttonSizeX, buttonSizeY);

		if (container) {
			container.setAttribute("style", "width: " + (btnSize * columnsCount) + "px; height: " + (btnSize * rowsCount) + "px;");
		}
		for (var i = 0; i < divs.length; i++) {
			divs[i].style.padding = buttonSpacing + "px";
			divs[i].style.borderRadius = buttonRadius + "px";
			divs[i].style.width = btnSize + "px";
			divs[i].style.height = btnSize + "px";
		}
		var buttonsEls = document.getElementsByClassName("action-button");
		for (var ii = 0; ii < buttonsEls.length; ii++) {
			buttonsEls[ii].style.borderRadius = ((buttonRadius / 2) / 100) * btnSize + "px";
		}
	}

	// ============================================================
	//  Cookie helpers
	// ============================================================
	function setCookie(cname, cvalue, exdays) {
		var d = new Date();
		d.setTime(d.getTime() + exdays * 24 * 60 * 60 * 1000);
		var expires = "expires=" + d.toUTCString();
		document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
	}

	function getCookie(cname) {
		var name = cname + "=";
		var decodedCookie = decodeURIComponent(document.cookie);
		var ca = decodedCookie.split(";");
		for (var i = 0; i < ca.length; i++) {
			var c = ca[i];
			while (c.charAt(0) === " ") { c = c.substring(1); }
			if (c.indexOf(name) === 0) {
				return c.substring(name.length, c.length);
			}
		}
		return "";
	}

	// ============================================================
	//  Event handlers (press / release / long press)
	// ============================================================
	function onTouchStart(id) {
		if (!IsTouchDevice()) return;
		buttonPress(id);
	}
	function onTouchEnd(id) {
		if (!IsTouchDevice()) return;
		buttonPressRelease(id);
	}
	function onMouseDown(id) {
		if (IsTouchDevice()) return;
		buttonPress(id);
	}
	function onMouseUp(id) {
		if (IsTouchDevice()) return;
		buttonPressRelease(id);
	}

	function buttonPress(id) {
		longPress = false;
		pressTimer = window.setTimeout(function () {
			longPress = true;
			var jsonObj = { "Message": id, "Method": JsonMethod.BUTTON_LONG_PRESS };
			if (supportButtonReleaseLongPress) {
				doSend(JSON.stringify(jsonObj));
			}
		}, 1000);
		if (document.getElementById("col_" + id)) {
			document.getElementById("col_" + id).classList.toggle("pressed", true);
			document.getElementById("col_" + id).classList.toggle("release-transition", false);
		}
		var jsonObj = { "Message": id, "Method": JsonMethod.BUTTON_PRESS };
		doSend(JSON.stringify(jsonObj));
	}

	function buttonPressRelease(id) {
		clearTimeout(pressTimer);
		if (document.getElementById("col_" + id)) {
			document.getElementById("col_" + id).classList.toggle("pressed", false);
			document.getElementById("col_" + id).classList.toggle("release-transition", true);
		}
		var jsonObj = { "Message": id, "Method": JsonMethod.BUTTON_RELEASE };
		if (longPress) {
			jsonObj = { "Message": id, "Method": JsonMethod.BUTTON_LONG_PRESS_RELEASE };
		}
		if (supportButtonReleaseLongPress) {
			doSend(JSON.stringify(jsonObj));
		}
	}

	function doSend(message) {
		if (websocket && websocket.readyState === WebSocket.OPEN) {
			websocket.send(message);
		}
	}

	function IsTouchDevice() {
		return "ontouchstart" in window || navigator.msMaxTouchPoints;
	}

	// ============================================================
	//  JsonMethod enum
	// ============================================================
	var JsonMethod = {
		CONNECTED: "CONNECTED",
		GET_CONFIG: "GET_CONFIG",
		GET_BUTTONS: "GET_BUTTONS",
		UPDATE_BUTTON: "UPDATE_BUTTON",
		UPDATE_LABEL: "UPDATE_LABEL",
		BUTTON_DONE: "BUTTON_DONE",
		REQUEST_PIN: "REQUEST_PIN",
		GET_ICONS: "GET_ICONS",
		BUTTON_PRESS: "BUTTON_PRESS",
		BUTTON_LONG_PRESS: "BUTTON_LONG_PRESS",
		BUTTON_RELEASE: "BUTTON_RELEASE",
		BUTTON_LONG_PRESS_RELEASE: "BUTTON_LONG_PRESS_RELEASE",
		KEEPALIVE: "KEEPALIVE"
	};

	// ============================================================
	//  DOM Ready
	// ============================================================
	$(document).ready(function () {
		// Apply i18n
		i18n.apply();

		reconnectDelay = 3000;
		// Init dark mode
		loadDarkPreference();

		// Connection status indicator: inject into header area
		var connectContainer = document.getElementById("connect-container");
		if (connectContainer) {
			var statusIndicator = document.createElement("div");
			statusIndicator.id = "connection-status";
			statusIndicator.className = "connection-status disconnected";
			statusIndicator.innerHTML =
				'<span class="status-dot disconnected"></span>' +
				'<span class="status-text">' + i18n.getString("disconnected") + '</span>';
			connectContainer.parentNode.insertBefore(statusIndicator, connectContainer);
		}

		// Reconnecting overlay
		var overlay = document.createElement("div");
		overlay.id = "reconnecting-overlay";
		overlay.className = "d-none";
		overlay.style.cssText =
			"position:fixed;top:0;left:0;width:100%;height:100%;background:rgba(0,0,0,0.7);z-index:9999;" +
			"display:flex;align-items:center;justify-content:center;";
		overlay.innerHTML =
			'<div style="text-align:center;color:white;">' +
			'<div class="spinner-border text-light mb-3" role="status"></div>' +
			'<h5><span>' + i18n.getString("reconnecting") + '</span></h5>' +
			'</div>';
		document.body.appendChild(overlay);

		// Dark mode click handler
		document.getElementById("btn-dark").addEventListener("click", toggleDark);

		// Init PWA UI
		updatePwaUi();

		// Connection form
		$('form[name="connect"]').on("submit", function (e) {
			e.preventDefault();
			var host = $(this).find('input[name="inputHost"]');
			var port = $(this).find('input[name="inputPort"]');
			connect("ws://" + host.val() + ":" + port.val() + "/");
		});

		// Client ID cookie
		if (getCookie("clientId")) {
			clientId = getCookie("clientId");
		} else {
			clientId = Math.random().toString(36).substr(2, 9);
			setCookie("clientId", clientId, 365);
		}

		document.getElementById("labelVersion").innerHTML = version;
		document.getElementById("client-id").innerHTML = clientId;

		// Recent connections
		if (getCookie("recentConnections")) {
			recentConnections = JSON.parse(getCookie("recentConnections"));
			if (recentConnections.length < 1) {
				document.getElementById("col-recent-connections").classList.toggle("d-none", true);
			} else {
				document.getElementById("col-recent-connections").classList.toggle("d-none", false);
			}
			for (var i = 0; i < recentConnections.length; i++) {
				(function (connUrl) {
					var recentConnectionItemRow = document.createElement("div");
					recentConnectionItemRow.classList.add("row");
					recentConnectionItemRow.classList.add("mb-2");

					var recentConnectionItem = document.createElement("div");
					recentConnectionItem.classList.add("col");
					recentConnectionItem.classList.add("recent-connection-item");
					recentConnectionItem.classList.add("text-left");
					recentConnectionItem.addEventListener("click", function () {
						connect(connUrl);
					});
					var recentConnectionUrl = document.createElement("h6");
					recentConnectionUrl.classList.add("my-auto");
					recentConnectionUrl.innerHTML = connUrl;
					recentConnectionItem.appendChild(recentConnectionUrl);

					var btnRemoveRecentConnection = document.createElement("button");
					btnRemoveRecentConnection.classList.add("col-auto");
					btnRemoveRecentConnection.classList.add("btn");
					btnRemoveRecentConnection.classList.add("btn-danger");
					btnRemoveRecentConnection.classList.add("ml-1");
					btnRemoveRecentConnection.style.minWidth = "50px";
					btnRemoveRecentConnection.innerHTML = "X";
					btnRemoveRecentConnection.addEventListener("click", function () {
						if (recentConnections.indexOf(connUrl) > -1) {
							recentConnections.splice(recentConnections.indexOf(connUrl), 1);
							setCookie("recentConnections", JSON.stringify(recentConnections), 365);
						}
						window.location.reload(false);
					});

					recentConnectionItemRow.appendChild(recentConnectionItem);
					recentConnectionItemRow.appendChild(btnRemoveRecentConnection);
					document.getElementById("recent-connections").appendChild(recentConnectionItemRow);
				})(recentConnections[i]);
			}
		}
	});

})();
