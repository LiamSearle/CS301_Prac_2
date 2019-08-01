switch (cpu.ir) {         // execute
          case PVM.nop:           // no operation
            break;
          case PVM.dsp:           // decrement stack pointer (allocate space for variables)
            cpu.sp -= mem[cpu.pc++];
            break;
          case PVM.ldc:           // push constant value
            mem[--cpu.sp] = mem[cpu.pc++];
            break;
          case PVM.lda:           // push local address
            mem[--cpu.sp] = cpu.fp - 1 - mem[cpu.pc++];
            break;
          case PVM.ldv:           // dereference 
            mem[cpu.sp] = mem[mem[cpu.sp]];
            break;
          case PVM.sto:           // store
            tos = mem[cpu.sp++]; mem[mem[cpu.sp++]] = tos;
            break;
          case PVM.ldxa:          // heap array indexing
            tos = mem[cpu.sp++]; mem[cpu.sp] = mem[cpu.sp] + tos;
            break;
          case PVM.inpi:          // integer input
            mem[mem[cpu.sp++]] = data.ReadInt();
            break;
          case PVM.inpc:          // char input
            mem[mem[cpu.sp++]] = data.ReadChar();
            break;
          case PVM.prni:          // integer output
//            if (tracing) results.Write(padding);
            results.Write(mem[cpu.sp++], 0);
//            if (tracing) results.WriteLine();
            break;
          case PVM.prnc:          // integer output
            results.Write((char)mem[cpu.sp++], 0);
            break;
          case PVM.inpb:          // boolean input
            mem[mem[cpu.sp++]] = data.ReadBool() ? 1 : 0;
            break;
          case PVM.prnb:          // boolean output
//            if (tracing) results.Write(padding);
            if (mem[cpu.sp++] != 0) results.Write(" true  "); else results.Write(" false ");
//            if (tracing) results.WriteLine();
            break;
          case PVM.prns:          // string output
//            if (tracing) results.Write(padding);
            loop = mem[cpu.pc++];
            while (mem[loop] != 0) {
              results.Write((char) mem[loop]); loop--;
            }
//            if (tracing) results.WriteLine();
            break;
          case PVM.prnl:          // newline
            results.WriteLine();
            break;
          case PVM.neg:           // integer negation
            mem[cpu.sp] = -mem[cpu.sp];
            break;
          case PVM.add:           // integer addition
            tos = mem[cpu.sp++]; mem[cpu.sp] += tos;
            break;
          case PVM.sub:           // integer subtraction
            tos = mem[cpu.sp++]; mem[cpu.sp] -= tos;
            break;
          case PVM.mul:
            tos = mem[cpu.sp++];
            sos = mem[cpu.sp];           // integer multiplication
            int freeSpace = (memSize - cpu.hp - (memSize - cpu.sp));
            if((freeSpace / tos) > sos)
            {
               mem[cpu.sp] *= tos;
            }
            else
            {
                ps = badVal;
            }
            break;
          case PVM.div:           // integer division (quotient)
            tos = mem[cpu.sp++];
            if (tos == 0)
            {
                ps = divZero;
            }
            else{
               mem[cpu.sp] /= tos;
            }
            break;
          case PVM.rem:           // integer division (remainder)
            tos = mem[cpu.sp++]; mem[cpu.sp] %= tos;
            break;
          case PVM.not:           // logical negation
            mem[cpu.sp] = mem[cpu.sp] == 0 ? 1 : 0;
            break;
          case PVM.and:           // logical and
            tos = mem[cpu.sp++]; mem[cpu.sp] &= tos;
            break;
          case PVM.or:            // logical or
            tos = mem[cpu.sp++]; mem[cpu.sp] |= tos;
            break;
          case PVM.ceq:           // logical equality
            tos = mem[cpu.sp++]; mem[cpu.sp] = mem[cpu.sp] == tos ? 1 : 0;
            break;
          case PVM.cne:           // logical inequality
            tos = mem[cpu.sp++]; mem[cpu.sp] = mem[cpu.sp] != tos ? 1 : 0;
            break;
          case PVM.clt:           // logical less
            tos = mem[cpu.sp++]; mem[cpu.sp] = mem[cpu.sp] <  tos ? 1 : 0;
            break;
          case PVM.cle:           // logical less or equal
            tos = mem[cpu.sp++]; mem[cpu.sp] = mem[cpu.sp] <= tos ? 1 : 0;
            break;
          case PVM.cgt:           // logical greater
            tos = mem[cpu.sp++]; mem[cpu.sp] = mem[cpu.sp] >  tos ? 1 : 0;
            break;
          case PVM.cge:           // logical greater or equal
            tos = mem[cpu.sp++]; mem[cpu.sp] = mem[cpu.sp] >= tos ? 1 : 0;
            break;
          case PVM.brn:           // unconditional branch
            cpu.pc = mem[cpu.pc++];
            break;
          case PVM.bze:           // pop top of stack, branch if false
            int target = mem[cpu.pc++];
            if (mem[cpu.sp++] == 0) cpu.pc = target;
            break;
          case PVM.anew:          // heap array allocation
            int size = mem[cpu.sp];
            mem[cpu.sp] = cpu.hp;
            cpu.hp += size;
            break;
          case PVM.halt:          // halt
            ps = finished;
            break;
          case PVM.stk:           // stack dump (debugging)
            StackDump(results, pcNow);
            break;
          case PVM.heap:           // heap dump (debugging)
            HeapDump(results, pcNow);
            break;
          case PVM.ldc_0:         // push constant 0
            mem[--cpu.sp] = 0;
            break;
          case PVM.ldc_1:         // push constant 1
            mem[--cpu.sp] = 1;
            break;
          case PVM.ldc_2:         // push constant 2
            mem[--cpu.sp] = 2;
            break;
          case PVM.ldc_3:         // push constant 3
            mem[--cpu.sp] = 3;
            break;
          case PVM.lda_0:         // push local address 0
            mem[--cpu.sp] = cpu.fp - 1 - 0;                
            break;
          case PVM.lda_1:         // push local address 1
            mem[--cpu.sp] = cpu.fp - 1 - 1;
            break;
          case PVM.lda_2:         // push local address 2
            mem[--cpu.sp] = cpu.fp - 1 - 2;
            break;
          case PVM.lda_3:         // push local address 3
            mem[--cpu.sp] = cpu.fp - 1 - 3;
            break;
          case PVM.ldl:           // push local value
            mem[--cpu.sp] = cpu.fp - 1 - mem[cpu.pc++];
            mem[cpu.sp] = mem[mem[cpu.sp]];
            break;
          case PVM.ldl_0:         // push value of local variable 0
            mem[--cpu.sp] = cpu.fp - 1 - 0;
            mem[cpu.sp] = mem[mem[cpu.sp]];
            break;
          case PVM.ldl_1:         // push value of local variable 1
            mem[--cpu.sp] = cpu.fp - 1 - 1;
            mem[cpu.sp] = mem[mem[cpu.sp]];
            break;
          case PVM.ldl_2:         // push value of local variable 2
            mem[--cpu.sp] = cpu.fp - 1 - 2;
            mem[cpu.sp] = mem[mem[cpu.sp]];
            break;
          case PVM.ldl_3:         // push value of local variable 3
            mem[--cpu.sp] = cpu.fp - 1 - 3;
            mem[cpu.sp] = mem[mem[cpu.sp]];
            break;
          case PVM.stl:           // store local value
			mem[--cpu.sp] = cpu.fp - 1 - mem[cpu.pc++];  
			mem[mem[cpu.sp++]] = mem[cpu.sp++];
			break;
          case PVM.stlc:          // store local value
          case PVM.stl_0:         // pop to local variable 0		
		    mem[--cpu.sp] = cpu.fp - 1 - 0; 
			mem[mem[cpu.sp++]] = mem[cpu.sp++];
			break;
          case PVM.stl_1:         // pop to local variable 1
			mem[--cpu.sp] = cpu.fp - 1 - 1; 
			mem[mem[cpu.sp++]] = mem[cpu.sp++];
			break;
          case PVM.stl_2:         // pop to local variable 2	
			mem[--cpu.sp] = cpu.fp - 1 - 2; 
			mem[mem[cpu.sp++]] = mem[cpu.sp++];
			break;
          case PVM.stl_3:         // pop to local variable 3
			mem[--cpu.sp] = cpu.fp - 1 - 3; 
			mem[mem[cpu.sp++]] = mem[cpu.sp++];
			break;
          case PVM.stoc:          // character checked store
          //case PVM.inpc:          // character input         // character output
          case PVM.cap:
          tos = mem[cpu.sp++];
            if (96 < tos && tos < 123)
            {
              mem[--cpu.sp] = (tos - 32);    
            }
            else
            {
                mem[--cpu.sp] = (tos);
            }
      break;
                     // toUpperCase
          case PVM.low:
          tos = mem[cpu.sp++];
            if (64 < tos && tos < 91)
            {
              mem[--cpu.sp] = (tos + 31);    
            }
            else
            {
                ps = badOp;
            }
      break;         // toLowerCase
          case PVM.islet:
          tos = mem[cpu.sp++];
          if (64 < tos && tos < 91 || 96 < tos && tos < 123)
          {
              mem[--cpu.sp] = 1;
          }else
          {
              mem[--cpu.sp] = 0;   
          }       // isLetter
      break;
          case PVM.inc:                         //NOT DONE PROPERLY
          tos = mem[cpu.sp++];           // ++  //NOT DONE PROPERLY 
          mem[--cpu.sp] = (tos += 1);           //NOT DONE PROPERLY
      break;                                    //NOT DONE PROPERLY
          case PVM.dec:                         //NOT DONE PROPERLY 
          tos = mem[cpu.sp++];           // --  //NOT DONE PROPERLY
          mem[--cpu.sp] = (tos -= 1);           //NOT DONE PROPERLY
      break;                                    
          default:                // unrecognized opcode
            ps = badOp;
            break;
        }
      } while (ps == running);
      TimeSpan ts = timer.Elapsed;

      // Format and display the TimeSpan value.
      string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                                         ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
      Console.WriteLine("\n\n" + ops + " operations.  Run Time " + elapsedTime + "\n\n");
      if (ps != finished) PostMortem(results, pcNow);
      timer.Reset();
      timer.Stop();
    } // PVM.Emulator

    public static void QuickInterpret(int codeLen, int initSP) {
    // Interprets the codeLen instructions stored in mem, with stack pointer
    // initialized to initSP.  Use StdIn and StdOut without asking
      Console.WriteLine("\nHit <Enter> to start");
      Console.ReadLine();
      bool tracing = false;
      InFile data = new InFile("");
      OutFile results = new OutFile("");
      Emulator(0, codeLen, initSP, data, results, false, false, false);
    } // PVM.QuickInterpret

    public static void Interpret(int codeLen, int initSP) {
    // Interactively opens data and results files.  Then interprets the codeLen
    // instructions stored in mem, with stack pointer initialized to initSP
      Console.Write("\nTrace execution (y/N/q)? ");
      char reply = (Console.ReadLine() + " ").ToUpper()[0];
      bool traceStack = false, traceHeap = false;
      if (reply != 'Q') {
        bool tracing = reply == 'Y';
        if (tracing) {
          Console.Write("\nTrace Stack (y/N)? ");
          traceStack = (Console.ReadLine() + " ").ToUpper()[0] == 'Y';
          Console.Write("\nTrace Heap (y/N)? ");
          traceHeap = (Console.ReadLine() + " ").ToUpper()[0] == 'Y';
        }
        Console.Write("\nData file [STDIN] ? ");
        InFile data = new InFile(Console.ReadLine());
        Console.Write("\nResults file [STDOUT] ? ");
        OutFile results = new OutFile(Console.ReadLine());
        Emulator(0, codeLen, initSP, data, results, tracing, traceStack, traceHeap);
        results.Close();
        data.Close();
      }
    } // PVM.Interpret

    public static void ListCode(string fileName, int codeLen) {
    // Lists the codeLen instructions stored in mem on a named output file
      int i, j, n;

      if (fileName == null) return;
      OutFile codeFile = new OutFile(fileName);

      /* ------------- The following may be useful for debugging the interpreter
      i = 0;
      while (i < codeLen) {
        codeFile.Write(mem[i], 5);
        if ((i + 1) % 15 == 0) codeFile.WriteLine();
        i++;
      }
      codeFile.WriteLine();

      ------------- */

      i = 0;
      codeFile.WriteLine("ASSEM\nBEGIN");
      while (i < codeLen) {
        int o = mem[i] % (PVM.nul + 1); // force in range
        codeFile.Write("  {");
        codeFile.Write(i, 5);
        codeFile.Write(" } ");
        codeFile.Write(mnemonics[o], -8);
        switch (o) {	  
          case PVM.brn:
          case PVM.bze:
          case PVM.dsp:
          case PVM.lda:
          case PVM.ldc:
		  case PVM.stl:
		  case PVM.ldl:
			i = (i + 1) % memSize; codeFile.Write(mem[i]);	//writes to code area
			break;  
          case PVM.prns:
            i = (i + 1) % memSize;
            j = mem[i]; codeFile.Write(" \"");
            while (mem[j] != 0) {
              switch (mem[j]) {
                case '\\' : codeFile.Write("\\\\"); break;
                case '\"' : codeFile.Write("\\\""); break;
                case '\'' : codeFile.Write("\\\'"); break;
                case '\b' : codeFile.Write("\\b"); break;
                case '\t' : codeFile.Write("\\t"); break;
                case '\n' : codeFile.Write("\\n"); break;
                case '\f' : codeFile.Write("\\f"); break;
                case '\r' : codeFile.Write("\\r"); break;
                default   : codeFile.Write((char) mem[j]); break;
              }
              j--;
            }
            codeFile.Write("\"");
            break;
        }
        i = (i + 1) % memSize;
        codeFile.WriteLine();
      }
      codeFile.WriteLine("END.");
      codeFile.Close();
    } // PVM.ListCode