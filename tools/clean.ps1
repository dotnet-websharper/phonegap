function rm-existing($f) {
  if (test-path $f) {
    rm -r -force $f
  }
}

rm-existing build/
rm-existing packages/
rm-existing tools/packages/
rm-existing node_modules/



