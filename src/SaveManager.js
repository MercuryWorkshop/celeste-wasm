import { downloadsave, uploadsave } from "./game.js";
import { zip, unzip } from "fflate";

export function SaveManager() {
  this.css = `
    flex-direction: column;
    justify-content: center;
    align-items: center;
    gap: 0.75rem;
    #dropzone {
      display: flex;

      flex-direction: column;
      align-items: center;
      justify-content: center;
      width: calc(min(100%, 40rem) - 6rem);
      height: auto;
      aspect-ratio: 1 / 1;
      border: 2px dashed var(--fg6);
      border-radius: 0.6rem;
      color: var(--fg6);
      transition: 0.3s;

      &.dragover {
        border-color: var(--accent);
        background-color: color-mix(in srgb, var(--accent) 5%, color-mix(in srgb, var(--surface2) 40%, transparent));
        transition: 0.3s;
      }

      h1 {
        margin: 0;
      }
    }

    #dl {
      width: calc(min(100%, 40rem) - 6rem);
    }
  `

  return html`
    <div class="flex">
    <div id="dropzone" class="gap"
    on:dragover=${(ev)=>{
      ev.preventDefault();
    }}
    on:dragenter=${()=>{
      document.getElementById("dropzone").classList.add("dragover"); // ZERO clue how the native dreamland API works so I'll just use getElementById
    }}
    on:dragleave=${()=>{
      document.getElementById("dropzone").classList.remove("dragover");
    }}
    on:drop=${async(ev)=>{
      // boilerplate stolen from MDN :P
        ev.preventDefault();
        var file;
        if (ev.dataTransfer.items) {
          // Use DataTransferItemList interface to access the file(s)
          // [...ev.dataTransfer.items].forEach((item, i) => {
          //   // If dropped items aren't files, reject them
          //   if (item.kind === "file") {
          //     const file = item.getAsFile();
          //     console.log(`… file[${i}].name = ${file.name}`);

          //   }
          // });
          if (ev.dataTransfer.items.length > 1) {
            alert("Please only drop one file at a time.");
            return;
          } else {
            file = ev.dataTransfer.items[0].getAsFile();
          }
        } else {
          // Use DataTransfer interface to access the file(s)
          // [...ev.dataTransfer.files].forEach((file, i) => {
          //   console.log(`… file[${i}].name = ${file.name}`);
          // });

          if (ev.dataTransfer.files.length > 1) {
            alert("Please only drop one file at a time.");
            return;
          } else {
            file = ev.dataTransfer.files[0];
          }
        }
        const reader = new FileReader();
        reader.onload = async function(e) {
          console.log(e.target.result);
          let unzipped = await new Promise((r) => unzip(new Uint8Array(data), (err, data) => r(data)));
          let files = Object.entries(unzipped);

          if (!dotnet.instance.Module.FS.analyzePath("/libsdl/Celeste").path) {
              dotnet.instance.Module.FS.mkdir("/libsdl/Celeste", 0o755);
              dotnet.instance.Module.FS.mkdir("/libsdl/Celeste/Saves", 0o755);
          }
          for (let [name, data] of files) {
              dotnet.instance.Module.FS.writeFile(`/libsdl/Celeste/Saves/${name}`, data);
          }
          await new Promise(r => dotnet.instance.Module.FS.syncfs(false, r));
          r();
        }

        reader.readAsArrayBuffer(file);

        document.getElementById("dropzone").classList.remove("dragover");
    }}
    >
          <div class="material-symbols-rounded" style="font-size: 5rem;">cloud_upload</div>
          <h1>Drag and drop file</h1>

          <div>
            <button id="upload" on:click=${async () => {
                  await uploadsave();
                }}>
                <span class="material-symbols-rounded">upload</span><span class="label">Upload from computer</span>
            </button>
          </div>
        </div>

        <button id="dl" on:click=${async () => {
          await downloadsave();
        }}>
           <span class="material-symbols-rounded">cloud_download</span><span class="label">Download current save</span>
        </button>
    </div>
  `
}
