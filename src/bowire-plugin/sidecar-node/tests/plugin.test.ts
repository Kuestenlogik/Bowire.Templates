import { describe, it, expect } from "vitest";
import { MyProtocol } from "../src/plugin.js";

describe("MyProtocol", () => {
  it("exposes a DemoService.Echo stub in discover()", () => {
    const services = new MyProtocol().discover();
    expect(services).toHaveLength(1);
    expect(services[0]!.name).toBe("DemoService");
    expect(services[0]!.methods).toHaveLength(1);
    expect(services[0]!.methods[0]!.name).toBe("Echo");
  });

  it("echoes the first body entry back through invoke()", () => {
    const result = new MyProtocol().invoke("", "", "", ['"hi"'], false, {});
    expect(result.status).toBe("OK");
    expect(result.response).toBe('{"echoed":"hi"}');
  });
});
