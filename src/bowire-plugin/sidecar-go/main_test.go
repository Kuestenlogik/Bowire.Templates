package main

import (
	"context"
	"testing"

	"github.com/Kuestenlogik/Bowire.Sdk.Go/plugin"
)

func TestDiscover_ExposesDemoServiceEchoStub(t *testing.T) {
	services, err := myProtocol{}.Discover(context.Background(), "", false)
	if err != nil {
		t.Fatalf("Discover: %v", err)
	}
	if len(services) != 1 {
		t.Fatalf("want 1 service, got %d", len(services))
	}
	if services[0].Name != "DemoService" {
		t.Errorf("want DemoService, got %s", services[0].Name)
	}
	if len(services[0].Methods) != 1 || services[0].Methods[0].Name != "Echo" {
		t.Errorf("want one Echo method, got %+v", services[0].Methods)
	}
}

func TestInvoke_EchoesFirstBodyEntry(t *testing.T) {
	result, err := myProtocol{}.Invoke(context.Background(), plugin.InvokeRequest{Body: []string{`"hi"`}})
	if err != nil {
		t.Fatalf("Invoke: %v", err)
	}
	if result.Status != "OK" {
		t.Errorf("want OK, got %s", result.Status)
	}
	if result.Response != `{"echoed":"hi"}` {
		t.Errorf("response mismatch: %q", result.Response)
	}
}
