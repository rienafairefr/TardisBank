import * as React from "react";
import * as ReactDOM from "react-dom";
import { Host } from "./host/Host";
import { initMessagingClient } from "./messaging/messagingClient";

const STORAGE_KEY = "tardis-token";
console.log("Hello");
initMessagingClient({ storageKey: STORAGE_KEY, rootUrl: "/api" });
ReactDOM.render(
  <Host storageKey={STORAGE_KEY} />,
  document.querySelector("#root")
);
