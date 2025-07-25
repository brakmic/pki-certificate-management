# detect OS and null device
ifeq ($(OS),Windows_NT)
	NULLDEV = nul
else
	NULLDEV = /dev/null
endif

.PHONY: init-network
init-network:
	@docker network inspect devnetwork >$(NULLDEV) 2>&1 || docker network create devnetwork
