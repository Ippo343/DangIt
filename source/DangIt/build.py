import os, sys, shutil

assert len(sys.argv)==3, "Please pass your KSP GameData directory as the first argument and your DangIt dll as the second"
game_data=sys.argv[1]
assembly=sys.argv[2]
assert os.path.exists(game_data), "Please pass a _valid_ GameData directory as the first argument"
assert os.path.exists(assembly), "Please pass a dll as the second argument"

contents=os.listdir(game_data)

if "DangIt" in contents:
	print("Removing DangIt dir...")
	shutil.rmtree(game_data+"/DangIt")

print("Copying Data")
shutil.copytree("Data", game_data+"/DangIt")

print("Copying DLL")
shutil.copyfile(assembly, game_data+"/DangIt/DangIt.dll")

print("Done!")