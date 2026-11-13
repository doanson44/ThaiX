(() => {
    const modelContext = document.modelContext;
    if (!modelContext || typeof modelContext.registerTool !== "function") {
        return;
    }

    const controller = new AbortController();
    window.addEventListener("pagehide", () => controller.abort(), { once: true });

    const fetchPublicText = async (path, signal) => {
        const response = await fetch(path, {
            method: "GET",
            headers: { Accept: "text/markdown, application/json" },
            credentials: "same-origin",
            signal
        });

        if (!response.ok) {
            throw new Error("ThaiX request failed with HTTP " + response.status + ".");
        }

        return await response.text();
    };

    const emptyInputSchema = {
        type: "object",
        properties: {},
        additionalProperties: false
    };

    const tools = [
        {
            name: "ThaiX_get_public_api_documentation",
            description: "Read the ThaiX API documentation, including the routes documented as anonymously accessible.",
            inputSchema: emptyInputSchema,
            annotations: { readOnlyHint: true },
            execute: async (_input, signal) => await fetchPublicText("/api.md", signal)
        },
        {
            name: "ThaiX_get_authentication_guide",
            description: "Read ThaiX authentication guidance and its documented JWT machine-client flow.",
            inputSchema: emptyInputSchema,
            annotations: { readOnlyHint: true },
            execute: async (_input, signal) => await fetchPublicText("/auth.md", signal)
        },
        {
            name: "ThaiX_get_health_status",
            description: "Retrieve the public ThaiX service health response.",
            inputSchema: emptyInputSchema,
            annotations: { readOnlyHint: true },
            execute: async (_input, signal) => await fetchPublicText("/health", signal)
        }
    ];

    for (const tool of tools) {
        modelContext.registerTool(tool, { signal: controller.signal }).catch((error) => {
            if (!controller.signal.aborted) {
                console.warn("Unable to register ThaiX WebMCP tool:", tool.name, error);
            }
        });
    }
})();
