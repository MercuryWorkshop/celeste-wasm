set FUNCS $(grep -r 'F_API' headers | grep -v '#define' | string split ':' -f 2 | tr -s " " | sed -e 's/;//' -e 's/ (/(/' -e 's/ $//')

for header in headers/*.h
	echo "#include \"$header\""
end
echo "#include <emscripten/proxying.h>"
echo "#include <emscripten/threading.h>"
echo "#include <assert.h>"

for func in $FUNCS
	set ret $(string split ' ' -f 1 $func)
	set name $(string split '(' -f 1 $func | string split ' ' -f 3)
	set args $(string split '(' -f 2 $func | sed 's/)//')
	set argsargs $(echo -n $args | sed -e 's/[a-zA-Z0-9_]* \**//g')
	set argc $(echo $argsargs | sed 's/[^,]//g' | string length)
	set argc $(math $argc + 1)

	echo typedef struct \{
	for arg in $(string split ',' $args)
		echo \t$(string trim $arg)\;
	end
	if ! test "$ret" = "void"
		echo \t$ret \*WRAP_RET\;
	end
	echo \} WRAP__struct_$name\;

	echo void WRAP__MAIN__$name\(void \*wrap_struct_ptr\) \{
	echo \tWRAP__struct_$name \*wrap_struct \= \(WRAP__struct_$name\*\)wrap_struct_ptr\;
	if ! test "$ret" = "void"
		echo \t\*\(wrap_struct\-\>WRAP_RET\) \= $name\(
	else
		echo \t$name\(
	end
	set i 0
	for arg in $(string split ',' $argsargs)
		set argtrimmed $(string trim $arg)
		echo -n \t\twrap_struct\-\>$argtrimmed
		if test $i -eq $(math $argc - 1)
			echo
		else
			echo \,
		end
		set i $(math $i + 1)
	end
	echo \t\)\;
	echo \}

	echo $ret F_API WRAP_$name\($args\)
	echo \{
	echo \t\/\/ \$func: `$func`
	echo \t\/\/ \$ret: `$ret`
	echo \t\/\/ \$name: `$name`
	echo \t\/\/ \$args: `$args`
	echo \t\/\/ \$argsargs: `$argsargs`
	echo \t\/\/ \$argc: `$argc`
	echo \t\/\/
	echo \t\/\/ return $name\($argsargs\)\;
	if ! test "$ret" = "void"
		echo \t$ret wrap_ret\;
	end
	echo \tWRAP__struct_$name wrap_struct \= \{
	for arg in $(string split ',' $argsargs)
		set argtrimmed $(string trim $arg)
		echo \t\t.$argtrimmed \= $argtrimmed\,
	end
	if ! test "$ret" = "void"
		echo \t\t.WRAP_RET \= \&wrap_ret
	end
	echo \t\}\;
	echo \tif \(\!emscripten_proxy_sync\(emscripten_proxy_get_system_queue\(\), emscripten_main_runtime_thread_id\(\), WRAP__MAIN__$name, \(void\*\)\&wrap_struct\)\) \{
	echo \t\temscripten_run_script\(\"console.error\(\'wrap.fish: failed to proxy $name\'\)\"\)\;
	echo \t\tassert\(0\)\;
	echo \t\}
	if ! test "$ret" = "void"
		echo \treturn wrap_ret\;
	end
	echo \}
	echo
end
