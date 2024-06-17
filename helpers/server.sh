SCRIPT_DIR=$( dirname -- "$( readlink -f -- "$0"; )"; )
cd "$1" || exit
python3 "${SCRIPT_DIR}/serve.py"
