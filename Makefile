help:
	@echo "Available targets:"
	@echo " setup - links githooks, inits & updates submodules"
	@echo " clean - remove all build files and reset Build.cs file"
	@echo " format - formats all source files according to our coding convention"

clean:
	rm -rf */{bin,obj}
	cp ../Build.cs Phoebe/Build.cs

setup:
	# Link hooks
	ln -fs ../../.githooks/pre-commit.sh .git/hooks/pre-commit
	# Get submodules
	git submodule init
	git submodule update
	# Ignore Build.cs changes
	git update-index --assume-unchanged Phoebe/Build.cs

format:
	@astyle --options=.astylerc --formatted --suffix=none $$(find Joey Ross Phoebe Emma Tests -type f -name '*.cs')

ios-patch-release:
	@echo "This is a workaround for iTunesConnect rejecting the app bundle due to GooglePlay component signatures."
	@echo "See: http://googledevelopers.blogspot.com.br/2014/09/an-important-announcement-for-ios.html"
	@echo ""
	rm -f Ross/bin/iPhone/AppStore/Ross.app/GooglePlus.bundle/GPPSignIn3PResources
	rm -f Ross/bin/iPhone/AppStore/Ross.app/GooglePlus.bundle/GPPCommonSharedResources.bundle/GPPCommonSharedResources
	rm -f Ross/bin/iPhone/AppStore/Ross.app/GooglePlus.bundle/GPPShareboxSharedResources.bundle/GPPShareboxSharedResources
	rm -f Ross/bin/iPhone/AppStore/Ross.app/GooglePlayGames.bundle/GooglePlayGames
	codesign -v -f -s "62A4FB9AD4F1924C5A58DB05FA1F6875E274E39E" "--resource-rules=Ross/bin/iPhone/AppStore/Ross.app/ResourceRules.plist" --entitlements "Ross/bin/iPhone/AppStore/Ross.xcent" "Ross/bin/iPhone/AppStore/Ross.app"
	cd Ross/bin/iPhone/AppStore/ && zip -r -y "/tmp/Toggl-iOS.zip" "Ross.app"
